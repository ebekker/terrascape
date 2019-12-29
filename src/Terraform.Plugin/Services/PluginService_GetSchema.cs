using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
            return Task.FromResult((GetProviderSchema.Types.Response)null);
        }
    }
}