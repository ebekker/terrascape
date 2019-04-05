using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.TFPlugin;
using HC.TFPlugin.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using Terrascape.WinLocalProvider.Registry;

namespace Terrascape.WinLocalProvider
{
    public partial class WinLocalProvider : IResourceProvider<RegistryKeyResource>
    {

        public ValidateResourceTypeConfigResult<RegistryKeyResource> ValidateConfig(
            ValidateResourceTypeConfigInput<RegistryKeyResource> input)
        {
            _log.LogDebug("{method}: ", nameof(ValidateConfig));
            _log.LogTrace("->input = {@input}", input);

            _log.LogTrace("  * ...Config: " + JsonConvert.SerializeObject(input.Config, Formatting.Indented));

            var result = new ValidateResourceTypeConfigResult<RegistryKeyResource>();

            if (!RegUtil.AllRootAliases.Contains(input.Config.Root))
                result.Error("invalid root, must be one of: "
                    + string.Join(" | ", RegUtil.AllRootAliases),
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

            _log.LogTrace("<-result = {@result}", result);
            return result;
        }

        public ReadResourceResult<RegistryKeyResource> Read(
            ReadResourceInput<RegistryKeyResource> input)
        {
            _log.LogDebug("{method}: ", nameof(Read));
            _log.LogTrace("->input = {@input}", input);

            _log.LogTrace("  * ...Current: " + JsonConvert.SerializeObject(input.CurrentState, Formatting.Indented));

            var result = DoRead(input);

            _log.LogTrace("<-result = {@result}", result);
            return result;
        }

        public PlanResourceChangeResult<RegistryKeyResource> PlanChange(
            PlanResourceChangeInput<RegistryKeyResource> input)
        {
            _log.LogDebug("{method}: ", nameof(PlanChange));
            _log.LogTrace("->input = {@input}", input);

            _log.LogDebug("  * .....Config: " + JsonConvert.SerializeObject(input.Config, Formatting.Indented));
            _log.LogDebug("  * ......Prior: " + JsonConvert.SerializeObject(input.PriorState, Formatting.Indented));
            _log.LogDebug("  * ...Proposed: " + JsonConvert.SerializeObject(input.ProposedNewState, Formatting.Indented));

            var result = new PlanResourceChangeResult<RegistryKeyResource>();
            var replace = new TFAttributePaths();

            result.RequiresReplace = ResolveRequiresReplace(input.PriorState, input.Config);
            result.PlannedState = input.ProposedNewState;
            result.PlannedPrivate = input.PriorPrivate;

            _log.LogTrace("<-result = {@result}", result);
            return result;
        }
        
        public ApplyResourceChangeResult<RegistryKeyResource> ApplyChange(
            ApplyResourceChangeInput<RegistryKeyResource> input)
        {
            _log.LogDebug("{method}: ", nameof(ApplyChange));
            _log.LogTrace("->input = {@input}", input);

            _log.LogDebug("  * ....Config: " + JsonConvert.SerializeObject(input.Config, Formatting.Indented));
            _log.LogDebug("  * .....Prior: " + JsonConvert.SerializeObject(input.PriorState, Formatting.Indented));
            _log.LogDebug("  * ...Planned: " + JsonConvert.SerializeObject(input.PlannedState, Formatting.Indented));

            var doDel = false;
            var doAdd = false;

            if (input.PriorState?.Root != input.PlannedState?.Root)
            {
                if (string.IsNullOrEmpty(input.PlannedState?.Root))
                    doDel = true;
                if (string.IsNullOrEmpty(input.PriorState?.Root))
                    doAdd = true;
            }

            if (input.PriorState?.Path != input.PlannedState?.Path)
            {
                if (string.IsNullOrEmpty(input.PlannedState?.Path))
                    doDel = true;
                if (string.IsNullOrEmpty(input.PriorState?.Path))
                    doAdd = true;
            }

            ApplyResourceChangeResult<RegistryKeyResource> result = null;

            if (doDel)
                result = DoDelete(input);
            if (doAdd)
                result = DoCreate(input);

            if (!doDel && !doAdd)
                result = DoUpdate(input);
            
            _log.LogTrace("<-result = {@result}", result);
            return result;
        }

        public ReadResourceResult<RegistryKeyResource> DoRead(
            ReadResourceInput<RegistryKeyResource> input)
        {
            return new ReadResourceResult<RegistryKeyResource>
            {
                // Nothing computed so we just copy
                // over everything we have been given
                NewState = input.CurrentState,
            };
        }

        private ApplyResourceChangeResult<RegistryKeyResource> DoCreate(
            ApplyResourceChangeInput<RegistryKeyResource> input)
        {
            var newRes = input.PlannedState;
            var pState = StateHelper.Deserialize<PrivateState>(input.PlannedPrivate)
                ?? new PrivateState();

            _log.LogInformation($"Creating registry key at [{newRes.Root}][{newRes.Path}]");
            RegistryKey root = RegUtil.ParseRootKey(newRes.Root);
            RegistryKey newKey = root.OpenSubKey(newRes.Path, true);
            if (newKey == null)
            {
                _log.LogInformation("Existing key does not exist, creating");
                newKey = root.CreateSubKey(newRes.Path, true);
                pState.RegistryKeyCreated = true;
            }

            using (var regKey = newKey)
            {
                ApplyValueDiffs(regKey, toAdd: newRes.Entries);
            }

            return new ApplyResourceChangeResult<RegistryKeyResource>
            {
                NewState = input.PlannedState,
                Private = StateHelper.Serialize(pState),
            };
        }

        private ApplyResourceChangeResult<RegistryKeyResource> DoUpdate(
            ApplyResourceChangeInput<RegistryKeyResource> input)
        {
            var oldRes = input.PriorState;
            var newRes = input.PlannedState;
            var pState = StateHelper.Deserialize<PrivateState>(input.PlannedPrivate);

            _log.LogInformation($"Updating registry key at [{newRes.Root}][{newRes.Path}]");
            RegistryKey root = RegUtil.ParseRootKey(newRes.Root);
            RegistryKey newKey = root.OpenSubKey(newRes.Path, true);
            if (newKey == null)
            {
                _log.LogInformation("Existing key does not exist, creating");
                newKey = root.CreateSubKey(newRes.Path, true);
                pState.RegistryKeyCreated = true;
            }

            var valDiffs = ResolveValueDiffs(oldRes.Entries, newRes.Entries);

            using (var regKey = newKey)
            {
                ApplyValueDiffs(regKey,
                    toDelKeys: valDiffs.toDel,
                    toDel: oldRes.Entries,
                    toAddKeys: valDiffs.toAdd,
                    toAdd: newRes.Entries,
                    toUpdKeys: valDiffs.toUpd,
                    toUpd: newRes.Entries);
            }

            return new ApplyResourceChangeResult<RegistryKeyResource>
            {
                NewState = input.PlannedState,
                Private = StateHelper.Serialize(pState),
            };
        }

        private ApplyResourceChangeResult<RegistryKeyResource> DoDelete(
            ApplyResourceChangeInput<RegistryKeyResource> input)
        {
            var oldRes = input.PriorState;
            var pState = StateHelper.Deserialize<PrivateState>(input.PlannedPrivate);

            _log.LogInformation($"Deleting registry key at [{oldRes.Root}][{oldRes.Path}]");
            RegistryKey root = RegUtil.ParseRootKey(oldRes.Root);
            RegistryKey oldKey = root.OpenSubKey(oldRes.Path, true);

            if (oldKey != null)
            {
                string delKey = null;
                using (var regKey = oldKey)
                {
                    ApplyValueDiffs(regKey, toDel: oldRes.Entries);

                    var subKeysLen = oldKey.GetSubKeyNames()?.Length??0;
                    var valuesLen = oldKey.GetValueNames()?.Length??0;
                    var forceDel = oldRes.ForceOnDelete??false;

                    if (!(pState?.RegistryKeyCreated??false))
                        _log.LogWarning("registry key was not created by us");
                    if (subKeysLen > 0)
                        _log.LogWarning($"registry key still has [{subKeysLen}] subkey(s)");
                    if (valuesLen > 0)
                        _log.LogWarning($"registry key still has [{valuesLen}] value(s)");
                    
                    if (!(pState?.RegistryKeyCreated??false) || subKeysLen > 0 || valuesLen > 0)
                    {
                        if (forceDel)
                        {
                            _log.LogWarning("forced delete specified");
                            delKey = oldKey.Name;
                        }
                        else
                        {
                            _log.LogWarning("reg key was not created by us or is not empty, SKIPPING delete");
                        }
                    }
                    else
                    {
                        delKey = oldKey.Name;
                    }
                }

                if (delKey != null)
                {
                    var openParent = RegUtil.OpenParentWithName(delKey, true);
                    if (openParent == null)
                    {
                        _log.LogWarning($"Cannot delete Registry Key [{delKey}], malformed path");
                    }
                    else
                    {
                        var (delRoot, parent, name) = openParent.Value;
                        _log.LogInformation($"Deleting Registry Key [{name}] under [{(parent??delRoot).Name}] ({delRoot.Name})");
                        using (var regKey = parent)
                        {
                            regKey.DeleteSubKeyTree(name, false);
                        }
                    }
                }
            }
            else
            {
                _log.LogInformation("Could not open existing, prior reg key, skipping");
            }

            return new ApplyResourceChangeResult<RegistryKeyResource>
            {
                NewState = input.PlannedState,
                Private = null,
            };
        }

        private IEnumerable<TFSteps> ResolveRequiresReplace(
            RegistryKeyResource oldState, RegistryKeyResource newState)
        {
            if (oldState?.Root != newState?.Root)
                yield return new TFSteps().Attribute("root");
            if (oldState?.Path != newState?.Path)
                yield return new TFSteps().Attribute("path");
        }

        private void ApplyValueDiffs(RegistryKey regKey,
            Dictionary<string, ArgumentRegValue> toDel = null,
            IEnumerable<string> toDelKeys = null,
            Dictionary<string, ArgumentRegValue> toAdd = null,
            IEnumerable<string> toAddKeys = null,
            Dictionary<string, ArgumentRegValue> toUpd = null,
            IEnumerable<string> toUpdKeys = null)
        {
            if (toDel != null)
            {
                foreach (var valName in (toDelKeys ?? toDel.Keys))
                {
                    _log.LogInformation("  * deleting value: [{@valueName}]",
                        valName);
                    regKey.DeleteValue(valName, false);
                }
            }

            // And Add and an Update are functionally the same (RegKey.SetValue(...))
            // but we split them out here for better bookkeeping and diagnostics

            if (toAdd != null)
            {
                foreach (var valName in (toAddKeys ?? toAdd.Keys))
                {
                    var valArg = toAdd[valName];
                    var valType = RegUtil.ParseValueKind(valArg.Type);
                    var valValue = RegUtil.ResolveValue(valArg);
                    _log.LogInformation("  * creating value: [{@valueName}]({@valueType})=[{@value}]",
                        valName, valType, valValue);
                    regKey.SetValue(valName, valValue, valType);
                }
            }

            if (toUpd != null)
            {
                foreach (var valName in (toUpdKeys ?? toUpd.Keys))
                {
                    var valArg = toUpd[valName];
                    var valType = RegUtil.ParseValueKind(valArg.Type);
                    var valValue = RegUtil.ResolveValue(valArg);
                    _log.LogInformation("  * updating value: [{@valueName}]({@valueType})=[{@value}]",
                        valName, valType, valValue);
                    regKey.SetValue(valName, valValue, valType);
                }
            }
        }

        private (IEnumerable<string> toDel, IEnumerable<string> toAdd, IEnumerable<string> toUpd) ResolveValueDiffs(
            Dictionary<string, ArgumentRegValue> oldState,
            Dictionary<string, ArgumentRegValue> newState)
        {
            if (oldState == null && newState == null)
                return (RegUtil.EmptyStrings, RegUtil.EmptyStrings, RegUtil.EmptyStrings);
            else if (oldState == null)
                return (RegUtil.EmptyStrings, newState.Keys, RegUtil.EmptyStrings);
            else if (newState == null)
                return (oldState.Keys, RegUtil.EmptyStrings, RegUtil.EmptyStrings);
            else
            {
                // Comparing entries in sorted key order
                var osEnts = oldState?.OrderBy(kv => kv.Key).ToArray();
                var nsEnts = newState?.OrderBy(kv => kv.Key).ToArray();
                var toDel = new List<string>();
                var toUpd = new List<string>();
                var toAdd = new List<string>();
                var osNdx = 0;
                var nsNdx = 0;
                while (osNdx < osEnts.Length && nsNdx < nsEnts.Length)
                {
                    var ose = osEnts[osNdx];
                    var nse = nsEnts[nsNdx];
                    var cmp = string.Compare(ose.Key, nse.Key);
                    if (cmp < 0)
                    {
                        toDel.Add(ose.Key);
                        ++osNdx;
                    }
                    else if (cmp > 0)
                    {
                        toAdd.Add(nse.Key);
                        ++nsNdx;
                    }
                    else
                    {
                        if (ose.Value.Type != nse.Value.Type
                            || ose.Value.Value != nse.Value.Value
                            || ose.Value.ValueBase64 != nse.Value.ValueBase64
                            || (!ose.Value.Values?.SequenceEqual(nse.Value.Values)??false))
                            toUpd.Add(ose.Key);
                        ++osNdx;
                        ++nsNdx;
                    }
                }
                while (osNdx < osEnts.Length)
                    toDel.Add(osEnts[osNdx++].Key);
                while (nsNdx < nsEnts.Length)
                    toAdd.Add(nsEnts[nsNdx++].Key);

                return (toDel, toAdd, toUpd);
            }
        }

        class PrivateState
        {
            public bool RegistryKeyCreated { get; set; }
        }
    }
}