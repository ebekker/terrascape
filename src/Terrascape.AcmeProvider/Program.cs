
using System;
using System.Threading.Tasks;
using HC.TFPlugin;
using Microsoft.Extensions.Logging;

namespace Terrascape.AcmeProvider
{
    class Program
    {
        // public static ILoggerFactory LoggerFactory { get; private set; }
        // public static IServiceCollection Services { get; private set; }
        // public static IServiceProvider ServiceProvider { get; private set; }

        private static ILogger _log;

        static async Task Main(string[] args)
        {
            // LoggerFactory = new LoggerFactory();
            // Services = new ServiceCollection()
            // Services.AddLogging();
            // ServiceProvider = Services.BuildServiceProvider();

            _log = LogUtil.Create<Program>();
            _log.LogInformation("========================================================================");
            _log.LogInformation("Starting up...");
            var tscapeSession = Environment.GetEnvironmentVariable("TSCAPE_SESSION_START");
            if (!string.IsNullOrEmpty(tscapeSession))
                _log.LogInformation($"TSCAPE Session Start: {tscapeSession}");

            try
            {
                await TFPluginServer.BuildServer();
            }
            catch (Exception ex)
            {
                _log.LogInformation(ex, "Exception caught at ENTRY");
                throw;
            }
        }
    }
}
