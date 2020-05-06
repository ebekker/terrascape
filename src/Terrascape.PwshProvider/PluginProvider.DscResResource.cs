using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Terraform.Plugin;
using Terraform.Plugin.Attributes;
using Terraform.Plugin.Skills;

namespace Terrascape.PwshProvider
{
    [TFResource("pwsh_dsc_resource")]
    public class DscResResource : IDscRes
    {
        [TFArgument("module_name", Required = true)]
        public string ModuleName { get; set; }

        [TFArgument("module_version", Required = true)]
        public string ModuleVersion { get; set; }

        [TFArgument("type_name", Required = true)]
        public string TypeName { get; set; }

        [TFArgument("properties")]
        public Dictionary<string, string> Properties { get; set; }

        [TFArgument("computer_name", Optional = true)]
        public string ComputerName { get; set; }

        [TFComputed("results")]
        public Dictionary<string, string> Results { get; set; }

        [TFComputed("in_desired_state")]
        public bool? InDesiredState { get; set; }

        /// <summary>
        /// Indicates whether a reboot was required after
        /// the last time this resource was updated.
        /// </summary>
        [TFComputed("required_reboot")]
        public bool? RequiredReboot { get; set; }
    }

    public partial class PluginProvider :
        IResourceProvider<DscResResource>
    {
        public HasValidateResourceTypeConfig.Result<DscResResource> ValidateConfig(
            HasValidateResourceTypeConfig.Input<DscResResource> input)
        {
            var result = new HasValidateResourceTypeConfig.Result<DscResResource>();

            DscResValidateConfig(result, input.Config);

            return result;
        }

        public HasUpgradeResourceState.Result<DscResResource> UpgradeState(
            HasUpgradeResourceState.Input<DscResResource> input)
        {
            var result = new HasUpgradeResourceState.Result<DscResResource>();

            result.UpgradedState = input.RawState.DeserializeAsJson<DscResResource>();

            return result;
        }

        public HasReadResource.Result<DscResResource> Read(
            HasReadResource.Input<DscResResource> input)
        {
            var result = new HasReadResource.Result<DscResResource>();

            result.NewState = (DscResResource)DscResReadConfig(result, input.CurrentState);
            result.Private = input.Private;

            return result;
        }

        public HasPlanResourceChange.Result<DscResResource> PlanChange(
            HasPlanResourceChange.Input<DscResResource> input)
        {
            var result = new HasPlanResourceChange.Result<DscResResource>();

            result.PlannedState = input.ProposedNewState;
            result.PlannedPrivate = input.PriorPrivate;

            if (input.ChangeType == TFResourceChangeType.Delete)
            {
                _log.LogWarning("DELETE REQUESTED -- DSC Delete is still an unknown quantity -- WHAT DOES IT MEAN?");
            }
            else if (input.ChangeType == TFResourceChangeType.Update)
            {
                if (!DscResTestConfig(result, input.Config))
                {
                    // These may need to be recomputed
                    result.PlannedState.InDesiredState = null;
                    result.PlannedState.RequiredReboot = null;
                    result.PlannedState.Results = null;
                }
            }

            return result;
        }

        public HasApplyResourceChange.Result<DscResResource> ApplyChange(
            HasApplyResourceChange.Input<DscResResource> input)
        {
            var result = new HasApplyResourceChange.Result<DscResResource>();

            if (input.ChangeType == TFResourceChangeType.Delete)
            {
                _log.LogWarning("DELETE REQUESTED -- DSC Delete is still an unknown quantity -- WHAT DOES IT MEAN?");
            }
            else
            {
                result.NewState = (DscResResource)DscResApplyConfig(result, input.Config, out bool reboot);
                result.NewState.RequiredReboot = reboot;
                result.Private = input.PlannedPrivate;
            }

            return result;
        }
    }
}