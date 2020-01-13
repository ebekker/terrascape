using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Terraform.Plugin.KVExample
{
    public partial class PluginProvider
    {
        private readonly ILogger _log;

        private ProviderConfig _config;

        private string _effectivePath;
        private string _fullPath;

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true,
            // Converters = {
            //     Util.TerraformEntityJsonConverterFactory.Instance
            // },
        };

        public PluginProvider(ILogger<PluginProvider> log)
        {
            _log = log;
            _log.LogInformation("Plugin Provider constructed");
        }

        private void LogInput(object input,
            [CallerMemberName]string method = null)
        {
            _log.LogInformation("[{method}]<<<Input: {@input}",
                method, JsonSerializer.Serialize(input, _jsonOpts));
        }

        private void LogResult(object result,
            [CallerMemberName]string method = null)
        {
            _log.LogInformation("[{method}]>>>Result: {@result}",
                method, JsonSerializer.Serialize(result, _jsonOpts));
        }
    }
}
