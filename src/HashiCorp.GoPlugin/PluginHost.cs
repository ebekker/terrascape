using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using HashiCorp.GoPlugin.Health;
using HashiCorp.GoPlugin.PKI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HashiCorp.GoPlugin
{
    public class PluginHost
    {
        public const int CoreProtocolVersion = 1;
        public const string NetworkType = "tcp";
        public const string ConnectionProtocol = "grpc";

        private CancellationTokenSource _cancelSource;

        private ILogger<PluginHost> _log;

        private WebApplicationHost _app;

        private X509Certificate2 _cert;

        public string AppProtocolVersion { get; set; } = "1";

        public string ListenHost { get; set; } = "localhost";

        public int ListenPort { get; set; } = 3000;

        public IServerPKIDetails PKI { get; set; }

        public IServiceProvider Services => _app?.Services;

        public void Prepare(string[] args)
        {
            if (_app != null)
            {
                throw new InvalidOperationException("host has already been built");
            }

            var builder = WebApplicationHost.CreateDefaultBuilder(args);
            _cert = PKI?.ToCertificate();

            builder.Http.ConfigureKestrel(kestrelOptions =>
            {
                var hosts = Dns.GetHostAddresses(ListenHost);
                foreach (var h in hosts)
                {
                    kestrelOptions.Listen(h, ListenPort, listenOptions =>
                    {
                        listenOptions.Protocols =
                            Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                        
                        if (_cert != null)
                        {
                            listenOptions.UseHttps(_cert);
                        }
                    });
                }
            });

            builder.Services.AddGrpc();
            builder.ConfigureLogging((c, b) =>
            {
                // Reset the logging providers config because we can not have the
                // default behavior of console logging since the console is used to
                // communicate with the plugin client as per the go-plugin protocol
                b.ClearProviders();

                // Build up our own configuration
                b.AddConsole(config =>
                {
                    // For now just send all logging to STDERR
                    config.LogToStandardErrorThreshold = LogLevel.Trace;
                });
            });

            // Makes the Plugin Main accessible to GRPC Service impls
            builder.Services.AddSingleton<PluginHost>(this);
            // This forces the HealthService to be a singleton
            builder.Services.AddSingleton<Services.HealthService>();
            // builder.Services.AddSingleton<Services.KVService>();
        
            _app = builder.Build();
            _log = _app.Services.GetRequiredService<ILogger<PluginHost>>();

            // GRPC Health Service is part of the Go Plugin protocol
            _app.MapGrpcService<Services.HealthService>();

            // Now we get the health singleton and register
            var health = (IHealthService)_app.Services.GetRequiredService<Services.HealthService>();
            health.SetStatus("plugin", HealthStatus.Serving);

            // var kv = app.Services.GetRequiredService<Services.KVService>();
        }

        public void AddService<TService>() where TService : class
        {
            if (_app == null)
            {
                throw new InvalidOperationException("Host is not yet prepared");
            }
            if (_cancelSource != null)
            {
                throw new InvalidOperationException("Host has already been started");
            }

            _app.MapGrpcService<TService>();
        }

        public async Task StartHosting()
        {
            if (_cancelSource != null)
            {
                throw new InvalidOperationException("hosting has already been started");
            }

            _cancelSource = new CancellationTokenSource();

            // Start it up, wait up to a second to make sure it didn't
            // throw right away such as with failure a port binding failure
            var appRunTask = _app.RunAsync(_cancelSource.Token);
            try
            {
                appRunTask.Wait(1000);
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Failed to startup PluginHost, ABORTING");
                return;
            }

            await WriteHandshakeAsync();

            Console.CancelKeyPress += (o, e) => {
                _log.LogInformation("Got CANCEL request");
                StopHosting();
                _log.LogInformation("Initiated Plugin Stop");
            };
            Console.WriteLine("Running...");
            Console.WriteLine("Hit CTRL+C to exit.");

            _ = Task.Run(async () => {
                await Task.Delay(5 * 1000);
                StopHosting();
            });

            await appRunTask;
        }

        public void StopHosting()
        {
            _cancelSource?.Cancel();
            _cancelSource = null;
        }

        private async Task WriteHandshakeAsync()
        {
            var address = $"{ListenHost}:{ListenPort}";
            var handshake = string.Join("|",
                CoreProtocolVersion,
                AppProtocolVersion,
                NetworkType,
                address,
                ConnectionProtocol);

            if (_cert != null)
            {
                var certEncoded = Convert.ToBase64String(_cert.RawData,
                    Base64FormattingOptions.None);
                handshake += $"|{certEncoded}";
            }

            await Console.Out.WriteAsync($"{handshake}\n");
        }
    }
}