using System;
using System.Collections.Generic;
using HC.TFPlugin;
using HC.TFPlugin.Attributes;
using Microsoft.Extensions.Logging;
using Terrascape.AcmeProvider;

namespace Terrascape.AcmeProvider
{
    public partial class AcmeProvider :
        IHasValidateResourceTypeConfig<FileResource>,
        IHasPlanResourceChange<FileResource>,
        IHasApplyResourceChange<FileResource>,
        IHasReadResource<FileResource>
    {
        public void ValidateConfig(FileResource res)
        {
            _log.LogDebug("{method}: ", nameof(PlanChange));
            _log.LogTrace("->input = {@res}", res);


            _log.LogInformation("NOTE: AcmeProvider Validating Res Config...");
            _log.LogInformation($"  * res = [{Newtonsoft.Json.JsonConvert.SerializeObject(res)}]");
        }

        public PlanResourceChangeResult<FileResource> PlanChange(
            PlanResourceChangeInput<FileResource> input)
        {
            _log.LogDebug("{method}: ", nameof(PlanChange));
            _log.LogTrace("->input = {@input}", input);

            var result = new PlanResourceChangeResult<FileResource>()
            {
                PlannedState = input.ProposedNewState,
                RequiresReplace = ComputeFileResourceRequiresReplace(input.PriorState, input.Config),
            };
            _log.LogInformation("PLANNED.");

            _log.LogTrace("<-result = {@result}", result);
            return result;
        }

        public ReadResourceResult<FileResource> Read(
            ReadResourceInput<FileResource> input)
        {
            _log.LogDebug("{method}: ", nameof(Read));
            _log.LogTrace("->input = {@input}", input);

            var result = new ReadResourceResult<FileResource>
            {
                NewState = input.CurrentState,
            };
            _log.LogInformation("resolving NewState as CurrentState");

            _log.LogTrace("<-result = {@result}", result);
            return result;
        }

        public ApplyResourceChangeResult<FileResource> ApplyChange(
            ApplyResourceChangeInput<FileResource> input)
        {
            _log.LogDebug("{method}: ", nameof(ApplyChange));
            _log.LogTrace("->input = {@input}", input);

            var result = new ApplyResourceChangeResult<FileResource>
            {
                NewState = input.Config,
            };
            if (input.Config == null)
                _log.LogInformation("RES DELETED!");
            else if (string.IsNullOrEmpty(input.Config.Path))
                _log.LogInformation("RES PATH MISSING!");
            _log.LogInformation("APPLIED.");

            _log.LogTrace("<-result = {@result}", result);
            return result;
        }

        private IEnumerable<ValuePath> ComputeFileResourceRequiresReplace(
            FileResource priorState, FileResource config)
        {
            // Only the path triggers a new resource (logically)
            if (priorState?.Path != config?.Path)
                yield return new ValuePath().Attribute(nameof(FileResource.Path));
        }
    }
}
