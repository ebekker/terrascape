using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
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
        public override async Task<PrepareProviderConfig.Types.Response> PrepareProviderConfig(
            PrepareProviderConfig.Types.Request request, ServerCallContext context)
        {
            var response = new PrepareProviderConfig.Types.Response();

            var providerType =_schemaResolver.PluginDetails.Provider;
            var configType = _schemaResolver.GetProviderConfigurationSchema().Type;

            if (providerType.HasPrepareProviderConfigSkill(configType))
            {
                providerType.InvokePrepareProviderConfigSkill(
                    PluginProviderInstance,
                    configType,
                    writeInput: (inputType, input) => {
                        inputType.GetProperty(nameof(request.Config))
                            .SetValue(input, request.Config.UnmarshalFromDynamicValue(configType));
                    },
                    readResult: (resultType, result) => {
                        var diagnostics = ((TFDiagnostics)resultType
                            .GetProperty(nameof(response.Diagnostics))
                            .GetValue(result));
                        if (diagnostics.Count() > 0)
                            response.Diagnostics.Add(diagnostics.All());

                        response.PreparedConfig = resultType
                            .GetProperty(nameof(response.PreparedConfig))
                            .GetValue(result)
                            .MarshalToDynamicValue();
                    });
            }
            else
            {
                // Default prepared config to incoming config
                response.PreparedConfig = request.Config;
            }

            return await Task.FromResult(response);
        }
    }
}
