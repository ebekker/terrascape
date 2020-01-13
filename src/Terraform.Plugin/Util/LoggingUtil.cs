using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace Terraform.Plugin.Util
{
    public static class LoggingUtil
    {
        public static void ConfigureSerilog(this ILoggerFactory lf,
            string logDir = null, IConfiguration config = null,
            IReadOnlyDictionary<string, LogLevel> filters = null) =>
            lf.AttachSerilog(ConfigureSerilog(logDir, config).CreateLogger(), filters);

        public static void ConfigureSerilog(this ILoggingBuilder lb,
            string logDir = null, IConfiguration config = null,
            IReadOnlyDictionary<string, LogLevel> filters = null) =>
            lb.AttachSerilog(ConfigureSerilog(logDir, config).CreateLogger(), filters);

        public static void AttachSerilog(this ILoggerFactory lf, Serilog.ILogger logger,
            IReadOnlyDictionary<string, LogLevel> filters = null)
        {
            lf.AddSerilog(logger);

            var flSettings = new FilterLoggerSettings();
            foreach (var f in filters)
            {
                flSettings.Add(f.Key, f.Value);
            }
            lf.WithFilter(flSettings);
        }

        public static void AttachSerilog(this ILoggingBuilder lb, Serilog.ILogger logger,
            IReadOnlyDictionary<string, LogLevel> filters = null)
        {
            lb.AddSerilog(logger);

            if (filters != null)
            {
                foreach (var f in filters)
                {
                    lb.AddFilter<SerilogLoggerProvider>(f.Key, f.Value);
                }
            }
        }

        public static LoggerConfiguration ConfigureSerilog(string logDir = null, IConfiguration config = null)
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

            // AppSettings JSON can be configured as per:
            //  https://github.com/serilog/serilog-settings-configuration
            //loggerConfig.ReadFrom.Configuration(ConfigUtil.Configuration);
            if (config != null)
                loggerConfig.ReadFrom.Configuration(config);
            
            return loggerConfig;
        }
    }
}