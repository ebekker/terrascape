using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace HC.TFPlugin
{
    public static class ConfigUtil
    {
        public static IConfiguration Configuration { get; }

        static ConfigUtil()
        {
            var asmFile = new FileInfo(Assembly.GetEntryAssembly().Location);
            var asmDir = asmFile.DirectoryName;
            var asmFileName = asmFile.Name;
            var asmFileWOExt = asmFileName.Replace(asmFile.Extension, "");

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile($"{asmFileWOExt}.json", optional: true)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"{asmDir}/appsettings.json", optional: true)
                .Build();
        }
    }
}