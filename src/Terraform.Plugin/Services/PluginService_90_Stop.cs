using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terraform.Plugin.Attributes;
using Terraform.Plugin.Skills;
using Tfplugin5;

namespace Terraform.Plugin.Services
{
    public partial class PluginService
    {
        [TraceExec]
        public override Task<Stop.Types.Response> Stop(
            Stop.Types.Request request, Grpc.Core.ServerCallContext context)
        {
            var response = new Stop.Types.Response();

            var providerType = _schemaResolver.PluginDetails.Provider;

            if (providerType.HasStopSkill())
            {
                providerType.InvokeStopSkill(
                    PluginProviderInstance,
                    readResult: (resultType, result) => {
                        var error = ((HasStop.Result)result).Error;
                        if (!string.IsNullOrEmpty(error))
                            response.Error = error;
                    });                
            }

            _log.LogInformation("Stopping Plugin Server");
            _pluginServer.Stop();

            return Task.FromResult(response);
        }
    }
}