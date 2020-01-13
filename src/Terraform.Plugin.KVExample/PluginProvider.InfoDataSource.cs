using Terraform.Plugin.Attributes;
using HasVDSC = Terraform.Plugin.Skills.HasValidateDataSourceConfig;
using HasRDS = Terraform.Plugin.Skills.HasReadDataSource;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Terraform.Plugin.KVExample
{
    [TFDataSource("kv_info")]
    public class InfoDataSource
    {
        [TFComputed("path")]
        public string Path { get; set; }

        [TFComputed("full_path")]
        public string FullPath { get; set; }
    }

    public partial class PluginProvider :
        HasRDS.Skill<InfoDataSource>
    {
        public HasRDS.Result<InfoDataSource> Read(HasRDS.Input<InfoDataSource> input)
        {
            var result = new HasRDS.Result<InfoDataSource>();

            LogInput(input);

            result.State = new InfoDataSource
            {
                Path = _config?.Path ?? string.Empty,
                FullPath = _fullPath ?? string.Empty,
            };

            LogResult(result);

            return result;
        }
    }
}