using System;
using System.Linq;
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
        public override async Task<ApplyResourceChange.Types.Response> ApplyResourceChange(
            ApplyResourceChange.Types.Request request, ServerCallContext context)
        {
            var response = new ApplyResourceChange.Types.Response();

            var providerType = _schemaResolver.PluginDetails.Provider;
            var resType = _schemaResolver.GetResourceSchemas()[request.TypeName].Type;

            if (providerType.HasApplyResourceChangeSkill(resType))
            {
                providerType.InvokeApplyResourceChangeSkill(
                    PluginProviderInstance,
                    resType,
                    writeInput: (inputType, input) => WriteInput(resType, request, inputType, input),
                    readResult: (resultType, result) => ReadResult(resType, resultType, result, response)
                    );
            }
            else
            {
                _log.LogWarning("provider does not handle applying change for resource [{type}]", resType);
            }

            return await Task.FromResult(response);
        }

        private void WriteInput(Type resType, ApplyResourceChange.Types.Request request, Type inputType, object input)
        {
            var config = request.Config.UnmarshalFromDynamicValue(resType);
            var priorState = request.PriorState.UnmarshalFromDynamicValue(resType);
            var plannedPrivate = request.PlannedPrivate.ToByteArray();
            var plannedState = request.PlannedState.UnmarshalFromDynamicValue(resType);

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

            _log.LogDebug("Applying " + changeType.ToString().ToUpper());

            inputType.GetProperty(nameof(HasApplyResourceChange.Input<object>.ChangeType))
                .SetValue(input, changeType);
            inputType.GetProperty(nameof(request.Config))
                .SetValue(input, config);
            inputType.GetProperty(nameof(request.PriorState))
                .SetValue(input, priorState);
            inputType.GetProperty(nameof(request.PlannedPrivate))
                .SetValue(input, plannedPrivate);
            inputType.GetProperty(nameof(request.PlannedState))
                .SetValue(input, plannedState);
        }

        private void ReadResult(Type resType, Type resultType, object result, ApplyResourceChange.Types.Response response)
        {
            var diagnostics = ((TFDiagnostics)resultType
                .GetProperty(nameof(response.Diagnostics)).GetValue(result));
            var newPrivate = resultType
                .GetProperty(nameof(response.Private)).GetValue(result);
            var newState = resultType
                .GetProperty(nameof(response.NewState)).GetValue(result);

            if (diagnostics.Count() > 0)
                response.Diagnostics.Add(diagnostics.All());
            if (newPrivate != null)
                response.Private = ByteString.CopyFrom((byte[])newPrivate);
            if (newState != null)
                response.NewState = newState.MarshalToDynamicValue(resType);
        }
    }
}
