
using System.Runtime.InteropServices;
using HC.TFPlugin;
using HC.TFPlugin.Attributes;
using HC.TFPlugin.Diagnostics;
using Microsoft.Extensions.Logging;
using Terrascape.WinLocalProvider;
using Terrascape.WinLocalProvider.Registry;

[assembly: TFPlugin(
    typeof(WinLocalProvider),
    DataSources = new[] {
        typeof(RegistryKeyDataSource),
    },
    Resources = new[] {
        typeof(RegistryKeyResource),
    })]

namespace Terrascape.WinLocalProvider
{
    [TFProvider]
    public partial class WinLocalProvider : IHasConfigure
    {
        private readonly ILogger _log = LogUtil.Create<WinLocalProvider>();

        public ConfigureResult Configure(ConfigureInput input)
        {
            var result = new ConfigureResult();

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                result.Error("This resource is only compatible with Windows.");
            }

            return result;
        }
    }
}