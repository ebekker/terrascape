using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Terraform.Plugin.Attributes;
using Terraform.Plugin.Diagnostics;
using Terraform.Plugin.Skills;

namespace Terraform.Plugin.KVExample
{
    [TFProviderConfiguration]
    public class ProviderConfig
    {
        [TFArgument("path",
            Optional = true)]
        public string Path { get; set; }
    }

    public partial class PluginProvider :
        HasPrepareProviderConfig.Skill<ProviderConfig>,
        HasConfigure.Skill<ProviderConfig>,
        HasStop.Skill
    {
        public const string DefaultStoreName = "kv.json";

        public HasPrepareProviderConfig.Result<ProviderConfig> PrepareConfig(
            HasPrepareProviderConfig.Input<ProviderConfig> input)
        {
            var result = new HasPrepareProviderConfig.Result<ProviderConfig>();

            LogInput(input);

            _log.LogInformation("Preparing configuration");
            result.PreparedConfig = input.Config;

            LogResult(result);

            return result;
        }

        public HasConfigure.Result<ProviderConfig> Configure(
            HasConfigure.Input<ProviderConfig> input)
        {
            var result = new HasConfigure.Result<ProviderConfig>();

            LogInput(input);

            _config = input.Config;
            _effectivePath = _config.Path;
            if (string.IsNullOrEmpty(_effectivePath))
            {
                _log.LogInformation("Resolving KV store path to current directory");
                _effectivePath = Directory.GetCurrentDirectory();
                result.AddWarning("defaulting to local directory, default kv store name");
            }
            
            if (File.Exists(_effectivePath))
            {
                _fullPath = Path.GetFullPath(_effectivePath);
            }
            else
            {
                _log.LogInformation("Found no existing file");
                if (Directory.Exists(_effectivePath))
                {
                    _log.LogInformation("Found existing directory");
                    _fullPath = Path.GetFullPath(
                        Path.Combine(_effectivePath, DefaultStoreName));
                }
                else
                {
                    _log.LogInformation("Combining current with specified");
                    _fullPath = Path.GetFullPath(
                        Path.Combine(Directory.GetCurrentDirectory(), _effectivePath));
                }
            }

            _log.LogInformation("Resolved full_path to [{full_path}]", _fullPath);

            LogResult(result);

            return result;
        }

        public HasStop.Result Stop(HasStop.Input input)
        {
            var result = new HasStop.Result();

            _log.LogInformation("Stopping...");

            return result;
        }
    }
}