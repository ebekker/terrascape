using HC.TFPlugin;
using HC.TFPlugin.Attributes;
using Microsoft.Extensions.Logging;
using Terrascape.LocalProvider;

[assembly: TFPlugin(
    typeof(Terrascape.LocalProvider.LocalProvider),
    DataSources = new[] {
        typeof(SystemInfoDataSource),
    },
    Resources = new[] {
        typeof(FileResource),
    })]

namespace Terrascape.LocalProvider
{
    [TFProvider]
    public partial class LocalProvider : IHasConfigure
    {
        private readonly ILogger _log = LogUtil.Create<LocalProvider>();

        public ConfigureResult Configure(ConfigureInput input)
        {
            _log.LogDebug("{method}: ", nameof(Configure));
            _log.LogTrace("->input = {@input}", input);
            _log.LogTrace("->state = {@state}", this);

            // TODO: configure and return any validation errors
            var result = new ConfigureResult();

            _log.LogTrace("<-state = {@state}", this);
            _log.LogTrace("<-result = {@result}", result);
            return result;
        }
    }
}