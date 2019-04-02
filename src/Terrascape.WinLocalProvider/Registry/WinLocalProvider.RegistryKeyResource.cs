using System;
using System.Collections.Generic;
using System.Linq;
using HC.TFPlugin;
using HC.TFPlugin.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Terrascape.WinLocalProvider.Registry;

namespace Terrascape.WinLocalProvider
{
    public partial class WinLocalProvider : IResourceProvider<RegistryKeyResource>
    {

        public ValidateResourceTypeConfigResult<RegistryKeyResource> ValidateConfig(
            ValidateResourceTypeConfigInput<RegistryKeyResource> input)
        {
            var result = new ValidateResourceTypeConfigResult<RegistryKeyResource>();

            if (!RegUtil.AllRoots.Contains(input.Config.Root))
                result.Error("invalid root, must be one of: "
                    + string.Join(" | ", RegUtil.AllRoots),
                    steps: new TFSteps().Attribute("root"));
            
            if (input.Config.Entries?.Count == 0)
                result.Error("at least one entry must be specified",
                    steps: new TFSteps().Attribute("entries"));
            else
            {
                foreach (var e in input.Config.Entries)
                {
                    var valArgs = 0;
                    if (!string.IsNullOrEmpty(e.Value.Value))
                        ++valArgs;
                    if (!string.IsNullOrEmpty(e.Value.ValueBase64))
                        ++valArgs;
                    if (e.Value.Values?.Length > 0)
                        ++valArgs;

                    if (valArgs != 1)
                        result.Error("entry must specify exactly one value argument"
                            + $" (currently = {valArgs}): "
                            + string.Join(",", ArgumentRegValue.AllValueArguments),
                            steps: new TFSteps()
                                .Attribute("entries")
                                .Element(e.Key));
                }
            }

            return result;
        }

        public ReadResourceResult<RegistryKeyResource> Read(
            ReadResourceInput<RegistryKeyResource> input)
        {
            var result = new ReadResourceResult<RegistryKeyResource>();

            // Nothing computed so we just copy over everything we have
            result.NewState = input.CurrentState;

            return result;
        }

        public PlanResourceChangeResult<RegistryKeyResource> PlanChange(
            PlanResourceChangeInput<RegistryKeyResource> input)
        {
            _log.LogDebug("{method}: ", nameof(PlanChange));
            _log.LogTrace("->input = {@input}", input);

            _log.LogDebug("  Config: " + JsonConvert.SerializeObject(input.Config, Formatting.Indented));
            _log.LogDebug("   Prior: " + JsonConvert.SerializeObject(input.PriorState, Formatting.Indented));
            _log.LogDebug("Proposed: " + JsonConvert.SerializeObject(input.ProposedNewState, Formatting.Indented));

            var result = new PlanResourceChangeResult<RegistryKeyResource>();
            var replace = new TFAttributePaths();

            result.RequiresReplace = ResolveRequiresReplace(input.PriorState, input.Config);
            result.PlannedState = input.ProposedNewState;

            _log.LogTrace("<-result = {@result}", result);
            return result;
        }

        public ApplyResourceChangeResult<RegistryKeyResource> ApplyChange(
            ApplyResourceChangeInput<RegistryKeyResource> input)
        {
            var result = new ApplyResourceChangeResult<RegistryKeyResource>();

            result.NewState = input.PlannedState;

            return result;
        }

        private IEnumerable<TFSteps> ResolveRequiresReplace(
            RegistryKeyResource prior, RegistryKeyResource config)
        {
            if (prior?.Root != config?.Root)
                yield return new TFSteps().Attribute(nameof(RegistryKeyResource.Root));
            if (prior?.Path != config?.Path)
                yield return new TFSteps().Attribute(nameof(RegistryKeyResource.Path));
        }

        private (IEnumerable<string> toDel, IEnumerable<string> toAdd, IEnumerable<string> toUpd) ResolveDiffs(
            Dictionary<string, ArgumentRegValue> prior,
            Dictionary<string, ArgumentRegValue> config)
        {
            if (prior == null && config == null)
                return (null, null, null);
            else if (prior == null)
                return (null, config.Keys, null);
            else if (config == null)
                return (prior.Keys, null, null);
            else
            {
                // Comparing entries in sorted key order
                var pEnts = prior?.OrderBy(kv => kv.Key).ToArray();
                var cEnts = config?.OrderBy(kv => kv.Key).ToArray();
                var toDel = new List<string>();
                var toUpd = new List<string>();
                var toAdd = new List<string>();
                var pNdx = 0;
                var cNdx = 0;
                while (pNdx < pEnts.Length && cNdx < cEnts.Length)
                {
                    var p = pEnts[pNdx];
                    var c = cEnts[cNdx];
                    var cmp = string.Compare(p.Key, c.Key);
                    if (cmp < 0)
                        toDel.Add(p.Key);
                    else if (cmp > 0)
                        toAdd.Add(c.Key);
                    else
                    {
                        if (p.Value.Type != c.Value.Type
                            || p.Value.Value != c.Value.Value
                            || p.Value.ValueBase64 != c.Value.ValueBase64
                            || !p.Value.Values.SequenceEqual(c.Value.Values))
                            toUpd.Add(p.Key);
                    }
                }
                while (pNdx < pEnts.Length)
                    toDel.Add(pEnts[pNdx++].Key);
                while (cNdx < cEnts.Length)
                    toAdd.Add(cEnts[cNdx++].Key);

                return (toDel, toAdd, toUpd);
            }
        }
    }
}