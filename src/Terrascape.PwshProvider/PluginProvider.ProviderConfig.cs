using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Terraform.Plugin.Attributes;
using Terraform.Plugin.Skills;

namespace Terrascape.PwshProvider
{
    [TFProviderConfiguration]
    public class ProviderConfig
    {

    }

    public partial class PluginProvider :
        HasPrepareProviderConfig.Skill<ProviderConfig>,
        HasConfigure.Skill<ProviderConfig>
    {
        public HasPrepareProviderConfig.Result<ProviderConfig> PrepareConfig(HasPrepareProviderConfig.Input<ProviderConfig> input)
        {
            var result = new HasPrepareProviderConfig.Result<ProviderConfig>();

            // Nothing to prepare yet...

            return result;
        }

        public HasConfigure.Result<ProviderConfig> Configure(HasConfigure.Input<ProviderConfig> input)
        {
            var result = new HasConfigure.Result<ProviderConfig>();

            _ps = PowerShell.Create();
            _rs = _ps.Runspace;

            return result;
        }
    }
}