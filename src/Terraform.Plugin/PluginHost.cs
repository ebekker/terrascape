using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Terraform.Plugin
{
    public class PluginHost
    {
        public const int DefaultListenMinPort = 15_000;
        public const int DefaultListenMaxPort = 20_000;

        public static ILoggerFactory LoggerFactory { get; } = new LoggerFactory();

        private Microsoft.Extensions.Logging.ILogger _log;

        private string _appBaseDir;
        private string _procModFile;
        private string _procModDir;

        private string _listenHost = "localhost";
        private int _listenPort = DefaultListenMinPort;

        private PluginProtocol _proto;

        private IHost _host;

        public void Init(string[] args)
        {
            _appBaseDir = AppContext.BaseDirectory;
            _procModFile = Process.GetCurrentProcess().MainModule.FileName;
            _procModDir = Path.GetDirectoryName(_procModFile);

            ConfigureSerilog(LoggerFactory, _procModDir);
            _log = LoggerFactory.CreateLogger<PluginHost>();

            //DumpEnv();

            ResolveListenEndpoint();

            var hostBuilder = CreateHostBuilder(_listenHost, _listenPort, args);
            _host = hostBuilder.Build();
        }

        public async Task RunAsync()
        {
            var runTask = _host.RunAsync();
            _proto.Announce(_listenHost, _listenPort);
            await runTask;
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
            _proto = new PluginProtocol();
            _proto.Resolve();

            _log.LogInformation("Resolved Plugin Protocol port range: [{0}] - [{1}]",
                _proto.MinPort, _proto.MaxPort);

            if (_proto.MinPort < 0 || _proto.MaxPort < 0)
                _listenPort = FindFreePort(DefaultListenMinPort, DefaultListenMaxPort);
            else
                _listenPort = FindFreePort(_proto.MinPort, _proto.MaxPort);

            if (_listenPort <= 0)
                throw new Exception("Unable to find a free port in requested range");
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        private static IHostBuilder CreateHostBuilder(string listenHost, int listenPort,
            string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls($"https://{listenHost}:{listenPort}");
                    webBuilder.UseStartup<Startup>();
                });

        private static void ConfigureSerilog(ILoggerFactory lf, string logDir = null)
        {
            var libPath = Assembly.GetEntryAssembly().Location;
            var libDir = Path.GetDirectoryName(libPath);
            var libName = Path.GetFileNameWithoutExtension(libPath);

            if (logDir == null)
                logDir = libDir;
            var logPath = Path.Combine(logDir, $"{libName}-serilog.log");

            var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}";
            var loggerConfig = new Serilog.LoggerConfiguration()
                    .Enrich.FromLogContext()
                    // // .Destructure.ByTransforming<DynamicValue>(DestructureDynamicValue)
                    .MinimumLevel.Verbose()
                    .WriteTo.File(logPath
                        //,restrictedToMinimumLevel: LogEventLevel.Verbose)
                        ,outputTemplate: outputTemplate);

            // // AppSettings JSON can be configured as per:
            // //  https://github.com/serilog/serilog-settings-configuration
            // loggerConfig.ReadFrom.Configuration(ConfigUtil.Configuration);

            LoggerFactory.AddSerilog(loggerConfig.CreateLogger());
        }

        // This is somewhat kludgey as there is a race condition in between
        // finding (and binding to) a free port and then releasing it so that
        // it can be rebound to by the gRPC Server class, but we have limited
        // options because we need to know the exact port to bind to when
        // configuring a WebHostBuilder with the listening endpoint
        private static int FindFreePort(int minPort, int maxPort)
        {
            if (maxPort < minPort)
                return -1;

            for (var curPort = minPort; curPort < maxPort; ++curPort)
            {
                var listener = new System.Net.Sockets.TcpListener(
                    System.Net.IPAddress.Loopback, curPort);
                try
                {
                    listener.Start();
                    listener.Stop();
                    return curPort;
                }
                catch (Exception)
                { }
            }

            return -1;
        }
    }
}