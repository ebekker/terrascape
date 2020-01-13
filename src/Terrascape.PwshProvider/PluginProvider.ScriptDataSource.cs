using System.Collections.Generic;
using System.Management.Automation;
using Terraform.Plugin;
using Terraform.Plugin.Attributes;
using Terraform.Plugin.Diagnostics;
using Terraform.Plugin.Skills;

namespace Terrascape.PwshProvider
{
    [TFDataSource("pwsh_script")]
    public class ScriptDataSource
    {
        [TFArgument("script", Required = true)]
        public string Script { get; set; }

        [TFArgument("inputs")]
        public Dictionary<string, string> Inputs { get; set; }

        [TFComputed("outputs")]
        public Dictionary<string, string> Outputs { get; set; }
    }

    public class ScriptContext
    {
        public static readonly IReadOnlyDictionary<string, string> EmptyInput =
            new Dictionary<string, string>();

        public ScriptContext(IReadOnlyDictionary<string, string> input)
        {
            Inputs = input ?? EmptyInput;
        }

        public IReadOnlyDictionary<string, string> Inputs { get; }

        public Dictionary<string, string> Outputs { get; } =
            new Dictionary<string, string>();
    }

    public partial class PluginProvider :
        IDataSourceProvider<ScriptDataSource>
    {
        public HasValidateDataSourceConfig.Result<ScriptDataSource> ValidateConfig(HasValidateDataSourceConfig.Input<ScriptDataSource> input)
        {
            var result = new HasValidateDataSourceConfig.Result<ScriptDataSource>();

            LogInput(input);

            if (string.IsNullOrWhiteSpace(input.Config.Script))
                result.AddError("Missing script content.");

            LogResult(result);

            return result;
        }

        public HasReadDataSource.Result<ScriptDataSource> Read(HasReadDataSource.Input<ScriptDataSource> input)
        {
            var result = new HasReadDataSource.Result<ScriptDataSource>();

            LogInput(input);

            // using (var ps = _ps.CreateNestedPowerShell())
            using (var ps = PowerShell.Create())
            {
                var ctx = new ScriptContext(input.Config.Inputs);
                ps.Runspace.SessionStateProxy.SetVariable("context", ctx);

                var values = ps.AddScript(input.Config.Script).Invoke();
                result.State = input.Config;
                result.State.Outputs = ctx.Outputs;
            }

            LogResult(result);

            return result;
        }
    }
}
