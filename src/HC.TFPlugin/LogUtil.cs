using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Tfplugin5;

namespace HC.TFPlugin
{
    public static partial class LogUtil
    {
        public static ILoggerFactory LoggerFactory { get; }
        
        static LogUtil()
        {
            LoggerFactory = new LoggerFactory();
          //LoggerFactory.AddConsole();
            ConfigureSerilog(LoggerFactory);

            //LoggerFactory.AddNLog();
        }

        public static ILogger Create(string name) => LoggerFactory.CreateLogger(name);
        public static ILogger Create(Type type) => LoggerFactory.CreateLogger(type);
        public static ILogger Create<T>() => LoggerFactory.CreateLogger<T>();

        // // static object DestructureDynamicValue(DynamicValue dv)
        // // {
        // //     string json = null;
        // //     if (dv.Json != null)
        // //     {
        // //         json = dv.Json.ToStringUtf8();
        // //     }

        // //     string msgpack = null;
        // //     if (dv.Msgpack != null)
        // //     {
        // //         msgpack = dv.Msgpack.ToStringUtf8();
        // //     }

        // //     return JsonConvert.SerializeObject(new { json, msgpack });
        // // }
    }
}

namespace HC.TFPlugin
{
    using Serilog;
    using Serilog.Events;

    public static partial class LogUtil
    {
        private static void ConfigureSerilog(ILoggerFactory lf)
        {
            var curDir = Directory.GetCurrentDirectory();
            var libPath = Assembly.GetEntryAssembly().Location;
            var libDir = Path.GetDirectoryName(libPath);
            var libName = Path.GetFileNameWithoutExtension(libPath);
          //var logPath = Path.Combine(libDir, $"{libName}-serilog.log");
            var logPath = Path.Combine(curDir, $"{libName}-serilog.log");

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
            loggerConfig.ReadFrom.Configuration(ConfigUtil.Configuration);

            LoggerFactory.AddSerilog(loggerConfig.CreateLogger());
        }
    }
}
