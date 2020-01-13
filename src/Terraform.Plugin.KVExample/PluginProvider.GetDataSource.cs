using System.Text.Json;
using Microsoft.Extensions.Logging;
using Terraform.Plugin.Attributes;
using Terraform.Plugin.Diagnostics;
using Terraform.Plugin.Skills;

namespace Terraform.Plugin.KVExample
{

    [TFDataSource("kv_get")]
    public class GetDataSource
    {
        [TFArgument("name",
            Required = true)]
        public string Name { get; set; }

        [TFArgument("required")]
        public bool? Required { get; set; }

        [TFComputed("value")]
        public string Value { get; set; }
    }


    public partial class PluginProvider : IDataSourceProvider<GetDataSource>
    {
        public HasValidateDataSourceConfig.Result<GetDataSource> ValidateConfig(
            HasValidateDataSourceConfig.Input<GetDataSource> input)
        {
            var result = new HasValidateDataSourceConfig.Result<GetDataSource>();

            LogInput(input);

            if (string.IsNullOrEmpty(input.Config.Name))
                result.AddError("attribute `name` is required");

            LogResult(result);

            return result;
        }

        public HasReadDataSource.Result<GetDataSource> Read(
            HasReadDataSource.Input<GetDataSource> input)
        {
            var result = new HasReadDataSource.Result<GetDataSource>();

            LogInput(input);

            // Copy over all the input values
            result.State = input.Config;

            // Computed for now...
            result.State.Value = $"[{input.Config.Name.ToUpper()}] ({System.IO.Directory.GetCurrentDirectory()})";

            LogResult(result);

            return result;
        }
    }
}