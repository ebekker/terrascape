using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Terraform.Plugin.Attributes;
using Terraform.Plugin.Diagnostics;
using Terraform.Plugin.Skills;
using Terraform.Plugin.Util;
using Tfplugin5;

namespace Terraform.Plugin.Services
{
    public partial class PluginService
    {
        [TraceExec]
        public override async Task<PlanResourceChange.Types.Response> PlanResourceChange(
            PlanResourceChange.Types.Request request, ServerCallContext context)
        {
            var response = new PlanResourceChange.Types.Response();

            var providerType = _schemaResolver.PluginDetails.Provider;
            var resType = _schemaResolver.GetResourceSchemas()[request.TypeName].Type;

            if (providerType.HasPlanResourceChangeSkill(resType))
            {
                providerType.InvokePlanResourceChangeSkill(
                    PluginProviderInstance,
                    resType,
                    writeInput: (inputType, input) => WriteInput(resType, request, inputType, input),
                    readResult: (resultType, result) => ReadResult(resType, resultType, result, response)
                    );
            }
            else
            {
                _log.LogWarning("provider does not handle planning change for resource [{type}]", resType);
            }

            return await Task.FromResult(response);
        }

        private void WriteInput(Type resType, PlanResourceChange.Types.Request request, Type inputType, object input)
        {
            var config = request.Config.UnmarshalFromDynamicValue(resType);
            var priorState = request.PriorState.UnmarshalFromDynamicValue(resType);
            var priorPrivate = request.PriorPrivate.ToByteArray();
            var proposedNewState = request.ProposedNewState.UnmarshalFromDynamicValue(resType);

            var changeType = TFResourceChangeType.Unknown;
            if (priorState != null)
                if (config == null)
                    changeType = TFResourceChangeType.Delete;
                else
                    changeType = TFResourceChangeType.Update;
            else
                if (config != null)
                    changeType = TFResourceChangeType.Create;
                else
                    _log.LogWarning("Planning NULL -> NULL : You Should Never See This!");

            _log.LogDebug("Planning " + changeType.ToString().ToUpper());

            inputType.GetProperty(nameof(HasPlanResourceChange.Input<object>.ChangeType))
                .SetValue(input, changeType);
            inputType.GetProperty(nameof(request.Config))
                .SetValue(input, config);
            inputType.GetProperty(nameof(request.PriorState))
                .SetValue(input, priorState);
            inputType.GetProperty(nameof(request.PriorPrivate))
                .SetValue(input, priorPrivate);
            inputType.GetProperty(nameof(request.ProposedNewState))
                .SetValue(input, proposedNewState);
        }

        private void ReadResult(Type resType, Type resultType, object result, PlanResourceChange.Types.Response response)
        {
            var diagnostics = ((TFDiagnostics)resultType
                .GetProperty(nameof(response.Diagnostics)) .GetValue(result));
            var plannedPrivate = resultType
                .GetProperty(nameof(response.PlannedPrivate)).GetValue(result);
            var plannedState = resultType
                .GetProperty(nameof(response.PlannedState)).GetValue(result);
            var requiresReplace = resultType
                .GetProperty(nameof(response.RequiresReplace)).GetValue(result);

            if (diagnostics.Count() > 0)
                response.Diagnostics.Add(diagnostics.All());
            if (plannedPrivate != null)
                response.PlannedPrivate = ByteString.CopyFrom((byte[])plannedPrivate);
            if (plannedState != null)
                response.PlannedState = plannedState.MarshalToDynamicValue(resType, withUnknowns: true);
            

            if (requiresReplace != null)
            {
                // Translates our internal representation of ValuePath to AttributePath
                var paths = (IEnumerable<TFSteps>)requiresReplace;
                response.RequiresReplace.Add(TFAttributePaths.ToPaths(paths));
            }
        }
    }
}