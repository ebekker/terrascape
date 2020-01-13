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
        public override async Task<ValidateDataSourceConfig.Types.Response> ValidateDataSourceConfig(
            ValidateDataSourceConfig.Types.Request request, ServerCallContext context)
        {
            var response = new ValidateDataSourceConfig.Types.Response();
            
            var providerType = _schemaResolver.PluginDetails.Provider;
            var resType = _schemaResolver.GetDataSourceSchemas()[request.TypeName].Type;

            if (providerType.HasValidateDataSourceConfigSkill(resType))
            {
                providerType.InvokeValidateDataSourceConfigSkill(
                    PluginProviderInstance,
                    resType,
                    writeInput: (inputType, input) => {
                        inputType.GetProperty(nameof(request.Config)).SetValue(input,
                            request.Config.UnmarshalFromDynamicValue(resType));
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