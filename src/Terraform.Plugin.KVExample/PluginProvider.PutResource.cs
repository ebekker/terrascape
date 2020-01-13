using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Terraform.Plugin.Attributes;
using Terraform.Plugin.Diagnostics;
using Terraform.Plugin.Skills;

namespace Terraform.Plugin.KVExample
{
    [TFResource("kv_put")]
    public class PutResource
    {
        [TFArgument("name", Required = true)]
        public string Name { get; set; }

        [TFArgument("value")]
        public string Value { get; set; }
    }

    public partial class PluginProvider :
        IResourceProvider<PutResource>,
        HasImportResourceState.Skill<PutResource>
    {
        public HasValidateResourceTypeConfig.Result<PutResource> ValidateConfig(
            HasValidateResourceTypeConfig.Input<PutResource> input)
        {
            var result = new HasValidateResourceTypeConfig.Result<PutResource>();

            LogInput(input);

            // Nothing more to validate

            LogResult(result);

            return result;
        }

        public HasReadResource.Result<PutResource> Read(
            HasReadResource.Input<PutResource> input)
        {
            var result = new HasReadResource.Result<PutResource>();

            LogInput(input);

            result.NewState = input.CurrentState;
            result.Private = input.Private;

            LogResult(result);

            return result;
        }

        public HasPlanResourceChange.Result<PutResource> PlanChange(
            HasPlanResourceChange.Input<PutResource> input)
        {
            var result = new HasPlanResourceChange.Result<PutResource>();

            LogInput(input);

            // _log.LogInformation("Got Inputs:");
            // _log.LogInformation("  * ChangeType = {@ChangeType}", input.ChangeType);
            // _log.LogInformation("  * Config = {@Config}", input.Config);
            // _log.LogInformation("  * PriorState = {@PriorState}", input.PriorState);
            // _log.LogInformation("  * ProposedNewState = {@ProposedNewState}", input.ProposedNewState);
            // _log.LogInformation("  * PriorPrivate = {@PriorPrivate}", input.PriorPrivate);

            result.PlannedState = input.ProposedNewState;
            result.PlannedPrivate = input.PriorPrivate;

            LogResult(result);

            return result;
        }

        public HasApplyResourceChange.Result<PutResource> ApplyChange(
            HasApplyResourceChange.Input<PutResource> input)
        {
            var result = new HasApplyResourceChange.Result<PutResource>();

            LogInput(input);

            // _log.LogInformation("Got Inputs:");
            // _log.LogInformation("  * ChangeType = {@ChangeType}", input.ChangeType);
            // _log.LogInformation("  * Config = {@Config}", input.Config);
            // _log.LogInformation("  * PriorState = {@PriorState}", input.PriorState);
            // _log.LogInformation("  * PlannedState = {@PlannedState}", input.PlannedState);
            // _log.LogInformation("  * PlannedPrivate = {@PlannedPrivate}", input.PlannedPrivate);

            Dictionary<string, string> kv;
            if (File.Exists(_fullPath))
            {
                var kvContent = File.ReadAllText(_fullPath);
                kv = JsonSerializer.Deserialize<Dictionary<string, string>>(kvContent);
            }
            else
            {
                kv = new Dictionary<string, string>();
            }

            // Deletes and Updates with Changed Keys
            if (input.ChangeType == TFResourceChangeType.Delete
                || (input.ChangeType == TFResourceChangeType.Update
                    && input.PriorState.Name != input.PlannedState.Name))
            {
                kv.Remove(input.PriorState.Name);
            }

            // Creates and Updates with Changed Keys or Values
            if (input.ChangeType == TFResourceChangeType.Create
                || (input.ChangeType == TFResourceChangeType.Update
                    && (input.PriorState.Name != input.PlannedState.Name
                        || input.PriorState.Value != input.PlannedState.Value)))
            {
                kv[input.PlannedState.Name] = input.PlannedState.Value;
            }

            File.WriteAllText(_fullPath, JsonSerializer.Serialize(kv));

            result.NewState = input.PlannedState;
            result.Private = input.PlannedPrivate;

            LogResult(result);

            return result;
        }

        public HasUpgradeResourceState.Result<PutResource> UpgradeState(
            HasUpgradeResourceState.Input<PutResource> input)
        {
            var result = new HasUpgradeResourceState.Result<PutResource>();

            LogInput(input);

            var json = Encoding.UTF8.GetString(input.RawState.RawBytes.Span);
            //var config = JsonSerializer.Deserialize<PutResource>(json, _jsonOpts);
            var config = input.RawState.DeserializeAsJson<PutResource>();

            // _log.LogInformation("Got Decoded Raw State:");
            // _log.LogInformation("  * RawState (JSON) = {@json}", json);
            // _log.LogInformation("  * Resource Config = {@config}", config);

            result.UpgradedState = config;

            LogResult(result);

            return result;
        }

        public HasImportResourceState.Result<PutResource> ImportResource(
            HasImportResourceState.Input<PutResource> input)
        {
            var result = new HasImportResourceState.Result<PutResource>();

            LogInput(input);

            Dictionary<string, string> kv;
            if (File.Exists(_fullPath))
            {
                var kvContent = File.ReadAllText(_fullPath);
                kv = JsonSerializer.Deserialize<Dictionary<string, string>>(kvContent);

                if (kv.ContainsKey(input.Id))
                {
                    result.ImportedResources = new[]
                    {
                        new TFImportedResource<PutResource>
                        {
                            State = new PutResource
                            {
                                Name = input.Id,
                                Value = kv[input.Id],
                            }
                        },
                    };
                }
                else
                {
                    result.AddWarning("Could not find KV entry",
                        $"no entry for key [{input.Id}] exists in KV store.");
                }

            }
            else
            {
                result.AddError("Missing KV store",
                    "KV store does not yet exist at the provider-specified path.");
            }

            LogResult(result);

            return result;
        }
    }
}