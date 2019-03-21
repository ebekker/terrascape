using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HC.TFPlugin;
using HC.TFPlugin.Attributes;
using HC.TFPlugin.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Terrascape.LocalProvider
{
    public partial class LocalProvider :
        IHasValidateResourceTypeConfig<FileResource>,
        IHasPlanResourceChange<FileResource>,
        IHasApplyResourceChange<FileResource>,
        IHasReadResource<FileResource>
    {
        public ValidateResourceTypeConfigResult<FileResource> ValidateConfig(
            ValidateResourceTypeConfigInput<FileResource> input)
        {
            _log.LogDebug("{method}: ", nameof(PlanChange));
            _log.LogTrace("->input = {@input}", input);

            var result = new ValidateResourceTypeConfigResult<FileResource>();

            var contentArgs = 0;
            if (!string.IsNullOrEmpty(input.Config.Content)) ++contentArgs;
            if (!string.IsNullOrEmpty(input.Config.ContentBase64)) ++contentArgs;
            if (!string.IsNullOrEmpty(input.Config.ContentPath)) ++contentArgs;
            if (!string.IsNullOrEmpty(input.Config.ContentUrl)) ++contentArgs;
            if (contentArgs != 1)
            {
                result.Error("exactly one of the content source arguments must be specified"
                    + $" ({FileResource.ContentSourceArgumentNames})");
            }

            var csumKey = input.Config.ComputeChecksum;
            if (!string.IsNullOrEmpty(csumKey)
                && csumKey != FileResource.MD5ChecksumKey
                && csumKey != FileResource.SHA1ChecksumKey
                && csumKey != FileResource.SHA256ChecksumKey
                && csumKey != FileResource.SourceCodeHashKey)
            {
                result.Error("invalid checksum key, allowed keys: "
                    + $" ({FileResource.AllowedChecksumKeys})");
            }

            _log.LogTrace("<-result = {@result}", result);
            return result;
        }

        public ReadResourceResult<FileResource> Read(
            ReadResourceInput<FileResource> input)
        {
            _log.LogDebug("{method}: ", nameof(Read));
            _log.LogTrace("->input = {@input}", input);

            _log.LogDebug("Current: " + JsonConvert.SerializeObject(input.CurrentState, Formatting.Indented));

            var result = new ReadResourceResult<FileResource>
            {
                NewState = new FileResource(),
            };

            ComputeState(input.CurrentState, result.NewState, result);

            _log.LogTrace("<-result = {@result}", result);
            return result;
        }

        public PlanResourceChangeResult<FileResource> PlanChange(
            PlanResourceChangeInput<FileResource> input)
        {
            _log.LogDebug("{method}: ", nameof(PlanChange));
            _log.LogTrace("->input = {@input}", input);

            _log.LogDebug("  Config: " + JsonConvert.SerializeObject(input.Config, Formatting.Indented));
            _log.LogDebug("   Prior: " + JsonConvert.SerializeObject(input.PriorState, Formatting.Indented));
            _log.LogDebug("Proposed: " + JsonConvert.SerializeObject(input.ProposedNewState, Formatting.Indented));

            var result = new PlanResourceChangeResult<FileResource>();
            if (input.ChangeType == ResourceChangeType.Update)
                result.RequiresReplace = ResolveRequiresReplace(input.PriorState, input.Config);

            result.PlannedState = new FileResource().CopyArgumentsFrom(input.Config);

            var newContent = input.ChangeType == ResourceChangeType.Create
                || (input.ChangeType == ResourceChangeType.Update
                    && (input.Config.Path != input.PriorState?.Path
                        || input.Config.SourceOfContent != input.PriorState?.SourceOfContent));

            if (!newContent)
            {
                result.PlannedState.FullPath = input.PriorState.FullPath;
                result.PlannedState.LastModified = input.PriorState.LastModified;
            }

            // If a checksum was requested, we indicate that the corresponding
            // checksum value property will be computed after the apply using null
            if (input.ChangeType != ResourceChangeType.Delete
                && !string.IsNullOrEmpty(input.Config.ComputeChecksum))
            {
                if (newContent
                    || input.Config.ComputeChecksum != input.PriorState.ComputeChecksum)
                {
                    _log.LogDebug("Reseting Checksum, to be computed ({prior} -> {config})",
                        input.PriorState?.ComputeChecksum,
                        input.Config?.ComputeChecksum);
                    result.PlannedState.Checksum = null;
                }
                else
                {
                    _log.LogDebug("Keeping Checksum");
                    result.PlannedState.Checksum = input.PriorState.Checksum;
                }
            }

            _log.LogTrace("<-result = {@result}", result);
            return result;
        }

        public ApplyResourceChangeResult<FileResource> ApplyChange(
            ApplyResourceChangeInput<FileResource> input)
        {
            _log.LogDebug("{method}: ", nameof(ApplyChange));
            _log.LogTrace("->input = {@input}", input);

            var result = new ApplyResourceChangeResult<FileResource>();
            if (input.ChangeType != ResourceChangeType.Delete)
                result.NewState = new FileResource().CopyArgumentsFrom(input.Config);

            var deleteOld = input.ChangeType != ResourceChangeType.Create
                && input.PlannedState.Path != input.PriorState.Path;
            var createNew = input.ChangeType != ResourceChangeType.Delete
                && (input.PlannedState.Path != input.PriorState?.Path
                || input.PlannedState.SourceOfContent != input.PriorState?.SourceOfContent);

            if (deleteOld)
            {
                // For Updates & Deletes, remove the old path
                _log.LogInformation("DELETING old path");
                File.Delete(input.PriorState.Path);
            }
            if (createNew)
            {
                // For Creates & Updates, create the new path
                _log.LogInformation("CREATING new path");
                if (!string.IsNullOrEmpty(input.Config.Content))
                {
                    File.WriteAllText(input.Config.Path,
                        input.Config.Content);
                }
                else if (!string.IsNullOrEmpty(input.Config.ContentBase64))
                {
                    File.WriteAllBytes(input.Config.Path,
                        Convert.FromBase64String(input.Config.ContentBase64));
                }
                else if (!string.IsNullOrEmpty(input.Config.ContentPath))
                {
                    File.Copy(input.Config.ContentPath, input.Config.Path, true);
                }
                else if (!string.IsNullOrEmpty(input.Config.ContentUrl))
                {
                    using (var wc = new WebClient())
                    {
                        wc.DownloadFile(input.Config.ContentUrl, input.Config.Path);
                    }
                }
            }

            ComputeState(input.Config, result.NewState, result);

            _log.LogTrace("<-result = {@result}", result);
            return result;
        }

        private IEnumerable<TFSteps> ResolveRequiresReplace(
            FileResource priorState, FileResource config)
        {
            // Only the path triggers a new resource (logically)
            if (priorState?.Path != config?.Path)
                yield return new TFSteps().Attribute(nameof(FileResource.Path));
        }

        private void ComputeState(FileResource state, FileResource newState, IHasDiagnostics diags)
        {
            if (string.IsNullOrEmpty(state?.Path))
                return;

            newState.CopyArgumentsFrom(state);
            var file = new FileInfo(state.Path);

            if (file.Exists)
            {
                newState.FullPath = file.FullName;
                newState.LastModified = file.LastWriteTime.ToString("r");

                var csumKey = newState.ComputeChecksum;
                if (!string.IsNullOrEmpty(csumKey))
                {
                    System.Security.Cryptography.HashAlgorithm csumAlgor = null;
                    switch (csumKey)
                    {
                        case FileResource.MD5ChecksumKey:
                            csumAlgor = System.Security.Cryptography.MD5.Create();
                            break;
                        case FileResource.SHA1ChecksumKey:
                            csumAlgor = System.Security.Cryptography.SHA1.Create();
                            break;
                        case FileResource.SHA256ChecksumKey:
                            csumAlgor = System.Security.Cryptography.SHA256.Create();
                            break;
                        case FileResource.SourceCodeHashKey:
                            csumAlgor = System.Security.Cryptography.MD5.Create();
                            break;
                        default:
                            diags.Error("invalid checksum key");
                            break;
                    }

                    if (csumAlgor != null)
                    {
                        byte[] csum;
                        using (csumAlgor)
                        using (var fs = file.OpenRead())
                        {
                            csum = csumAlgor.ComputeHash(fs);
                        }

                        if (csumKey == FileResource.SourceCodeHashKey)
                            newState.Checksum = Convert.ToBase64String(csum);
                        else
                            newState.Checksum = BitConverter.ToString(csum).Replace("-", "");
                    }
                }
            }
        }
    }
}
