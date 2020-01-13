using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HashiCorp.GoPlugin;
using HashiCorp.GoPlugin.PKI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Terraform.Plugin.Util;

namespace Terraform.Plugin
{
    public class TFPluginServer
    {
        public const string BadInvokeMessage = @"
This binary is a plugin. These are not meant to be executed directly.
Please execute the program that consumes these plugins, which will
load any plugins automatically
";
        public const int DefaultListenMinPort = 15_000;
        public const int DefaultListenMaxPort = 18_000;

        public static readonly IReadOnlyDictionary<string, LogLevel> DefaultLoggingFilters =
            new Dictionary<string, LogLevel>
            {
                { nameof(Microsoft), LogLevel.Warning },
                { nameof(Grpc), LogLevel.Warning },
                { "Microsoft.AspNetCore.Routing.EndpointMiddleware",
                    LogLevel.Information },
                { typeof(TFPluginProtocol).FullName, LogLevel.Information },
            };

        public static ILoggerFactory LoggerFactory { get; } = new LoggerFactory();

        private Microsoft.Extensions.Logging.ILogger _log;

        private string _appBaseDir;
        private string _procModFile;
        private string _procModDir;

        private string _listenHost = "localhost";
        private int _listenPort = DefaultListenMinPort;

        private TFPluginProtocol _proto;

        private PluginHost _pluginHost;

        public IReadOnlyDictionary<string, LogLevel> LoggingFilters { get; set; }

        public IServiceProvider Services => _pluginHost?.Services;

        public void Prepare(string[] args, PluginHost pluginHost = null)
        {
            _appBaseDir = AppContext.BaseDirectory;
            _procModFile = Process.GetCurrentProcess().MainModule.FileName;
            _procModDir = Path.GetDirectoryName(_procModFile);

            var logFilters = LoggingFilters ?? DefaultLoggingFilters;

            var seriLogger = LoggingUtil.ConfigureSerilog(_procModDir).CreateLogger();

            LoggerFactory.AttachSerilog(seriLogger, filters: logFilters);
            _log = LoggerFactory.CreateLogger<TFPluginServer>();
            _log.LogDebug("Logging initialized");
            _log.LogInformation("***********************************************************************************");
            _log.LogInformation("** Plugin Server Preparing...");
            _log.LogInformation("***********************************************************************************");

            //DumpEnv();

            _proto = new TFPluginProtocol();
            if (!_proto.PrepareHandshake())
            {
                if (!_proto.MagicCookieFound)
                {
                    Console.Error.WriteLine(BadInvokeMessage);
                    Environment.Exit(-1);
                    return;
                }
                if (!_proto.ProtocolVersionMatched)
                {
                    throw new Exception("Protocol version mistmatch");
                }
            }

            ResolveListenEndpoint();

            _pluginHost = pluginHost ?? NewPluginHost();
            _pluginHost.PKI = SimplePKIDetails.GenerateRSA();
            _pluginHost.ListenHost = _listenHost;
            _pluginHost.ListenPort = _listenPort;
            _pluginHost.AppProtocolVersion = _proto.AppProtocolVersion;
            _pluginHost.LoggingBuilderAction = lb =>
            {
                lb.ClearProviders();
                lb.AttachSerilog(seriLogger, filters: logFilters);
            };

            _pluginHost.PrepareHost(args);
            _pluginHost.AddServices(services =>
                {
                    services.AddTransient(typeof(Util.Fallback<>));

                    services.AddSingleton(this);
                    services.AddSingleton<ISchemaResolver, Services.SchemaResolver>();
                    services.AddSingleton<Services.PluginService>();
                });
            _pluginHost.BuildHostApp();
            _pluginHost.MapGrpcService<Services.PluginService>();
        }

        private PluginHost NewPluginHost()
        {
            return new PluginHost
            {
                GrpcBrokerEnabled = false,
            };
        }

        public Task RunAsync()
        {
            _log.LogInformation("Starting Plugin Host...");
            var runTask = _pluginHost.StartHosting();
            _log.LogInformation("started.");

            return runTask;
        }

        public void Stop()
        {
            _log.LogInformation("Stop request received...");
            _pluginHost.StopHosting();
            _log.LogInformation("Requested Plugin Host to stop");
        }

        private void DumpEnv()
        {
            _log.LogDebug($"appBaseDir=[{_appBaseDir}]");
            _log.LogDebug($"procModDir=[{_procModDir}]");

            var env = Environment.GetEnvironmentVariables();
            foreach (var envKey in env.Keys.Cast<string>().OrderBy(x => x))
            {
                _log.LogTrace($"* [{envKey}]=[{env[envKey]}]");
            }
        }

        private void ResolveListenEndpoint()
        {
            _log.LogInformation("Resolved Plugin Protocol port range: [{0}] - [{1}]",
                _proto.MinPort, _proto.MaxPort);

            // This is somewhat kludgey as there is a race condition in between
            // finding (and binding to) a free port and then releasing it so that
            // it can be rebound to by the gRPC Server class, but we have limited
            // options because we need to know the exact port to bind to when
            // configuring a WebHostBuilder with the listening endpoint
            if (_proto.MinPort < 0 || _proto.MaxPort < 0)
                _listenPort = Util.NetUtil.FindFreePort(DefaultListenMinPort, DefaultListenMaxPort);
            else
                _listenPort = Util.NetUtil.FindFreePort(_proto.MinPort, _proto.MaxPort);

            if (_listenPort <= 0)
                throw new Exception("Unable to find a free port in requested range");

            _log.LogInformation("Found free port in range: [{0}]", _listenPort);
        }

        // // Additional configuration is required to successfully run gRPC on macOS.
        // // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        // private IHostBuilder CreateHostBuilder(string listenHost, int listenPort, string[] args)
        // {
        //     return Host.CreateDefaultBuilder(args)
        //         .UseSerilog()
        //         .ConfigureWebHostDefaults(webBuilder =>
        //         {
        //             // webBuilder.UseUrls($"https://{listenHost}:{listenPort}");

        //             webBuilder.ConfigureKestrel(kestrelOptions => 
        //             {
        //                 kestrelOptions.Listen(IPAddress.Loopback, listenPort, listenOptions =>
        //                 {
        //                     listenOptions.UseHttps(_pki.ToCertificate());
        //                     listenOptions.Protocols =
        //                         Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        //                 });
        //                 // kestrelOptions.ConfigureHttpsDefaults(httpsOptions =>
        //                 // {
        //                 //     httpsOptions.ServerCertificate = _pki.ToCertificate();
        //                 //     httpsOptions
        //                 // });
        //             });

        //             // webBuilder.UseKestrel(options =>
        //             // {
        //             //     // options.ConfigureEndpointDefaults(epOptions =>
        //             //     // {
        //             //     //     epOptions.Protocols =
        //             //     //         Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        //             //     // });
        //             //     options.Listen(IPAddress.Loopback, listenPort, lisOptions =>
        //             //     {
        //             //         lisOptions.UseHttps(_pki.ToCertificate());
        //             //         lisOptions.Protocols =
        //             //             Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        //             //     });
        //             // });

        //             webBuilder.UseStartup<Startup>();
        //         });
        // }

    }
}