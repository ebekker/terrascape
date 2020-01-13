using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Terrascape.PwshProvider
{
    public partial class PluginProvider
    {
        private ILogger _log;
        
        private PowerShell _ps;
        private Runspace _rs;

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
            _log.LogInformation("PWSH Plugin Provider constructed");
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