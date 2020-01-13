using System;
using System.Threading.Tasks;
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
        public override async Task<UpgradeResourceState.Types.Response> UpgradeResourceState(
            UpgradeResourceState.Types.Request request, ServerCallContext context)
        {
            var response = new UpgradeResourceState.Types.Response();

            var providerType = _schemaResolver.PluginDetails.Provider;
            var resType = _schemaResolver.GetResourceSchemas()[request.TypeName].Type;

            if (providerType.HasUpgradeResourceStateSkill(resType))
            {
                providerType.InvokeUpgradeResourceStateSkill(
                    PluginProviderInstance,
                    resType,
                    writeInput: (inputType, input) => {
                        inputType.GetProperty(nameof(request.Version)).SetValue(input,
                            request.Version);
                        inputType.GetProperty(nameof(request.RawState)).SetValue(input,
                            new TFRawState(request.RawState.Json.ToByteArray()));
                    },
                    readResult: (resultType, result) => {
                        var diagnostics = ((TFDiagnostics)resultType.GetProperty(nameof(response.Diagnostics))
                            .GetValue(result));
                        if (diagnostics.Count() > 0)
                            response.Diagnostics.Add(diagnostics.All());

                        var upgradedState = resultType.GetProperty(nameof(response.UpgradedState))
                                .GetValue(result);
                        if (upgradedState != null)
                            response.UpgradedState = upgradedState.MarshalToDynamicValue(resType);
                    });
            }
            else
            {
                _log.LogWarning("provider does not handle applying change for resource [{type}]", resType);
            }

            return await Task.FromResult(response);
        }
    }
}
