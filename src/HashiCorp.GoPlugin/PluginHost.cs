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
        // https://github.com/hashicorp/go-plugin/blob/master/docs/guide-plugin-write-non-go.md#4-output-handshake-information
        public const int CoreProtocolVersion = 1;

        public const string UnixNetworkType = "unix";
        public const string TcpNetworkType = "tcp";

        public const string ConnectionProtocol = "grpc";

        private CancellationTokenSource _cancelSource;

        private ILogger<PluginHost> _log;

        private WebApplicationHostBuilder _builder;
        private WebApplicationHost _app;

        private X509Certificate2 _cert;

        /// <summary>
        /// Enable plugin support for client-initiated shutdown.
        /// </summary>
        /// <remarks>
        /// By default this feature is enabled, and when invoked
        /// by the plugin client, it will stop the plugin host.
        /// You can  override this behavior by registering a
        /// singleton instance of the <see cref="GrpControllerService" />
        /// and providing your own <c>OnShutdown</c> behavior.
        /// Alternatively you can disable this feature entirely.
        /// </remarks>
        /// <value></value>
        public bool GrpcControllerEnabled { get; set; } = true;

        public bool GrpcBrokerEnabled { get; set; } = true;

        // Possible future enhancement to explore unix network type
        // (now supported on Windows too!
        //    https://devblogs.microsoft.com/commandline/af_unix-comes-to-windows/
        //    https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.server.kestrel.core.kestrelserveroptions.listenunixsocket)
        public string NetworkType { get; } = TcpNetworkType;

        public string AppProtocolVersion { get; set; } = "1";

        public string ListenHost { get; set; } = "localhost";

        public int ListenPort { get; set; } = 3000;

        public IServerPKIDetails PKI { get; set; }

        public IServiceProvider Services => _app?.Services;

        public Action<ILoggingBuilder> LoggingBuilderAction { get; set; }

        public CancellationToken StopHostingToken => _cancelSource?.Token ?? CancellationToken.None;

        public void PrepareHost(string[] args)
        {
            if (_builder != null)
                throw new InvalidOperationException("host has already been prepared");

            _builder = WebApplicationHost.CreateDefaultBuilder(args);
            _cert = PKI?.ToCertificate();

            _builder.Http.ConfigureKestrel(kestrelOptions =>
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

            _builder.Services.AddGrpc();
            _builder.ConfigureLogging((hbc, lb) =>
            {
                if (LoggingBuilderAction != null)
                {
                    LoggingBuilderAction(lb);
                }
                else
                {
                    lb.AddFilter(nameof(Microsoft), LogLevel.Warning);
                    lb.AddFilter(nameof(Grpc), LogLevel.Warning);

                    // Reset the logging providers config because we cannot have the
                    // default behavior of console logging since the console is used to
                    // communicate with the plugin client as per the go-plugin protocol
                    lb.ClearProviders();

                    // Build up our own configuration
                    lb.AddConsole(config =>
                    {
                        // For now just send all logging to STDERR
                        config.LogToStandardErrorThreshold = LogLevel.Trace;
                    });
                }
            });

            // Makes the Plugin Main accessible to GRPC Service implementations
            _builder.Services.AddSingleton<PluginHost>(this);
            // This forces the HealthService to be a singleton
            _builder.Services.AddSingleton<Services.HealthService>();
            _builder.Services.AddSingleton<Services.GrpcBrokerService>();
            _builder.Services.AddSingleton<Services.GrpcControllerService>();
        }

        public void AddServices(Action<IServiceCollection> servicesConfiguration)
        {
            if (_builder == null)
                throw new InvalidOperationException("need to prepare first");
            if (_app != null)
                throw new InvalidOperationException("host app has already been built");

            servicesConfiguration(_builder.Services);
        }

        public void BuildHostApp()
        {
            if (_builder == null)
                throw new InvalidOperationException("need to prepare first");
            if (_app != null)
                throw new InvalidOperationException("host app has already been built");

            _app = _builder.Build();
            _log = _app.Services.GetRequiredService<ILogger<PluginHost>>();

            // GRPC Health Service is part of the Go Plugin protocol
            _app.MapGrpcService<Services.HealthService>();
            if (GrpcControllerEnabled)
            {
                _log.LogInformation("Registering GRPC Controller service");
                _app.MapGrpcService<Services.GrpcControllerService>();
            }
            if (GrpcBrokerEnabled)
            {
                _log.LogInformation("Registering GRPC Broker service");
                _app.MapGrpcService<Services.GrpcBrokerService>();
            }

            // Now we get the health singleton and register
            var health = (IHealthService)_app.Services.GetRequiredService<Services.HealthService>();
            health.SetStatus("plugin", HealthStatus.Serving);

            // var kv = app.Services.GetRequiredService<Services.KVService>();
        }

        public void MapGrpcService<TService>() where TService : class
        {
            if (_app == null)
                throw new InvalidOperationException("Host is not yet prepared");
            if (_cancelSource != null)
                throw new InvalidOperationException("Host has already been started");

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
                throw new Exception("Failed to startup plugin hosting", ex);
            }

            await WriteHandshakeAsync();
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