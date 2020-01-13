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
        public override async Task<Configure.Types.Response> Configure(
            Configure.Types.Request request, ServerCallContext context)
        {
            var response = new Configure.Types.Response();

            var providerType =_schemaResolver.PluginDetails.Provider;
            var configType = _schemaResolver.GetProviderConfigurationSchema().Type;

            if (providerType.HasConfigureSkill(configType))
            {
                providerType.InvokeConfigureSkill(
                    PluginProviderInstance,
                    configType,
                    writeInput: (type, input) => {
                        type.GetProperty(nameof(request.TerraformVersion))
                            .SetValue(input, request.TerraformVersion);
                        type.GetProperty(nameof(request.Config))
                            .SetValue(input, request.Config.UnmarshalFromDynamicValue(configType));
                    },
                    readResult: (resultType, result) => {
                        var diagnostics = ((TFDiagnostics)resultType
                            .GetProperty(nameof(response.Diagnostics))
                            .GetValue(result));
                        if (diagnostics.Count() > 0)
                            response.Diagnostics.Add(diagnostics.All());
                    });
            }
            
            return await Task.FromResult(response);
        }
    }
}
