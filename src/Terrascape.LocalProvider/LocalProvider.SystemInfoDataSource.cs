using System.Runtime.InteropServices;
using HC.TFPlugin;
using Microsoft.Extensions.Logging;

namespace Terrascape.LocalProvider
{
    public partial class LocalProvider : IDataSourceProvider<SystemInfoDataSource>
    {
        public ValidateDataSourceConfigResult<SystemInfoDataSource> ValidateConfig(ValidateDataSourceConfigInput<SystemInfoDataSource> input)
        { 
            return new ValidateDataSourceConfigResult<SystemInfoDataSource>();
        }

        public ReadDataSourceResult<SystemInfoDataSource> Read(ReadDataSourceInput<SystemInfoDataSource> input)
        {
            var result = new ReadDataSourceResult<SystemInfoDataSource>();
            var osPlatform = SystemInfoDataSource.OtherOSPlatform;
            foreach (var osp in SystemInfoDataSource.OSPlatforms)
            {
                if (RuntimeInformation.IsOSPlatform(osp))
                    osPlatform = osp.ToString();
            }

            result.State = new SystemInfoDataSource
            {
                ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                OSArchitecture = RuntimeInformation.OSArchitecture.ToString(),
                OSPlatform = osPlatform,
                OSDescription = RuntimeInformation.OSDescription,
                OSVersionString = System.Environment.OSVersion.VersionString,
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
            };

            return result;
        }
    }
}