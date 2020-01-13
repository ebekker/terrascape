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
        public override async Task<ReadResource.Types.Response> ReadResource(
            ReadResource.Types.Request request, ServerCallContext context)
        {
            var response = new ReadResource.Types.Response();

            var providerType = _schemaResolver.PluginDetails.Provider;
            var resType = _schemaResolver.GetResourceSchemas()[request.TypeName].Type;

            if (providerType.HasReadResourceSkill(resType))
            {
                providerType.InvokeReadResourceSkill(
                    PluginProviderInstance,
                    resType,
                    writeInput: (inputType, input) => {
                        inputType.GetProperty(nameof(request.CurrentState)).SetValue(input,
                            request.CurrentState.UnmarshalFromDynamicValue(resType));
                        inputType.GetProperty(nameof(request.Private)).SetValue(input,
                            request.Private?.ToByteArray());
                    },
                    readResult: (resultType, result) => {
                        var diagnostics = ((TFDiagnostics)resultType.GetProperty(nameof(response.Diagnostics))
                            .GetValue(result));
                        if (diagnostics.Count() > 0)
                            response.Diagnostics.Add(diagnostics.All());

                        var newState = resultType
                                .GetProperty(nameof(response.NewState))
                                .GetValue(result);
                        if (newState != null)
                            response.NewState = newState.MarshalToDynamicValue(resType);
                        
                        var prv = resultType
                                .GetProperty(nameof(response.Private))
                                .GetValue(result);
                        if (prv != null)
                            response.Private = ByteString.CopyFrom((byte[])prv);
                    });
            }
            else
            {
                _log.LogWarning("provider does not handle reading for resource [{type}]", resType);
            }

            return await Task.FromResult(response);
        }        
    }
}