using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using HC.TFPlugin.Attributes;
using Microsoft.Extensions.Logging;
using Tfplugin5;
using static Tfplugin5.AttributePath.Types;
using static Tfplugin5.AttributePath.Types.Step;

namespace HC.TFPlugin
{
    public partial class ProviderImpl
    {
        public override Task<Tfplugin5.GetProviderSchema.Types.Response> GetSchema(
            Tfplugin5.GetProviderSchema.Types.Request request, ServerCallContext context)
        {
            _log.LogDebug(">>>{method}>>>", nameof(GetSchema));
            _log.LogTrace($"->input[{nameof(request)}] = {{@request}}", request);
            _log.LogTrace($"->input[{nameof(context)}] = {{@context}}", context);

            try
            {
                var response = new Tfplugin5.GetProviderSchema.Types.Response();
                response.Provider = SchemaHelper.GetProviderSchema(PluginAssembly);
                response.DataSourceSchemas.Add(SchemaHelper.GetDataSourceSchemas());
                response.ResourceSchemas.Add(SchemaHelper.GetResourceSchemas());

                _log.LogTrace("Provider schema: {@schema}", response.Provider);

                _log.LogTrace("Data Source schemas: ({count})", response.DataSourceSchemas.Count);
                foreach (var rs in response.DataSourceSchemas)
                {
                    _log.LogInformation("  * {dsType}={@dsSchema}", rs.Key, rs.Value);
                }
                
                _log.LogTrace("Resource schemas: ({count})", response.ResourceSchemas.Count);
                foreach (var rs in response.ResourceSchemas)
                {
                    _log.LogInformation("  * {resType}={@resSchema}", rs.Key, rs.Value);
                }

                _log.LogTrace("<-result = {@response}", response);
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "<!exception = ");
                throw;
            }
        }
    }
}
