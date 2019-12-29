using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tfplugin5;

namespace Terraform.Plugin.Services
{
    public partial class PluginService
    {
        [TraceExec]
        public override Task<Stop.Types.Response> Stop(
            Stop.Types.Request request,
            Grpc.Core.ServerCallContext context)
        {
            var response = new Stop.Types.Response();

            return Task.FromResult(response);
        }
    }
}