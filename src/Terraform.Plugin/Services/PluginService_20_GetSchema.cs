using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terraform.Plugin.Attributes;
using Tfplugin5;

namespace Terraform.Plugin.Services
{
    public partial class PluginService
    {
        [TraceExec]
        public override Task<GetProviderSchema.Types.Response> GetSchema(
            GetProviderSchema.Types.Request request,
            Grpc.Core.ServerCallContext context)
        {
            var response = new GetProviderSchema.Types.Response();

            response.Provider = _schemaResolver.GetProviderConfigurationSchema().Schema;
            _log.LogInformation("Resolved Schema for Provider implemented by [{provider}]",
                _schemaResolver.PluginDetails.Provider);

            foreach (var schema in _schemaResolver.GetDataSourceSchemas().Values)
            {
                response.DataSourceSchemas.Add(schema.Name, schema.Schema);
                _log.LogInformation("Resolved Schema for Data Source [{name}]", schema.Name);
            }

            foreach (var schema in _schemaResolver.GetResourceSchemas().Values)
            {
                response.ResourceSchemas.Add(schema.Name, schema.Schema);
                _log.LogInformation("Resolved Schema for Data Source [{name}]", schema.Name);
            }

            return Task.FromResult(response);
        }
    }
}