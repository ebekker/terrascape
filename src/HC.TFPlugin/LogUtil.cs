using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;

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
          //var logPath = Path.Combine(libDir, $"{libName}-serilog.txt");
            var logPath = Path.Combine(curDir, $"{libName}-serilog.txt");

            var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}";

            LoggerFactory.AddSerilog(
                new Serilog.LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Verbose()
                    .WriteTo.File(logPath
                        //,restrictedToMinimumLevel: LogEventLevel.Verbose)
                        ,outputTemplate: outputTemplate)
                    .CreateLogger());
        }
    }
}
