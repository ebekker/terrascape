using System.Linq;
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
        public override async Task<ReadDataSource.Types.Response> ReadDataSource(
            ReadDataSource.Types.Request request, ServerCallContext context)
        {
            var response = new ReadDataSource.Types.Response();

            var providerType = _schemaResolver.PluginDetails.Provider;
            var resType = _schemaResolver.GetDataSourceSchemas()[request.TypeName].Type;

            if (providerType.HasReadDataSourceSkill(resType))
            {
                providerType.InvokeReadDataSourceSkill(
                    PluginProviderInstance,
                    resType,
                    writeInput: (inputType, input) => {
                        inputType.GetProperty(nameof(request.Config)).SetValue(input,
                            request.Config.UnmarshalFromDynamicValue(resType));
                    },
                    readResult: (resultType, result) => {
                        var diagnostics = ((TFDiagnostics)resultType.GetProperty(nameof(response.Diagnostics))
                            .GetValue(result));
                        if (diagnostics.Count() > 0)
                            response.Diagnostics.Add(diagnostics.All());

                        var newState = resultType.GetProperty(nameof(response.State))
                                .GetValue(result);
                        if (newState != null)
                            response.State = newState.MarshalToDynamicValue(resType);
                    });
            }
            else
            {
                _log.LogWarning("provider does not handle reading for data source [{type}]", resType);
            }

            return await Task.FromResult(response);
        }
    }
}
