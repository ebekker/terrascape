using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tfplugin5;

namespace Terraform.Plugin.Services
{
    /// <summary>
    /// This class implements the RPC service defined by the protobuf
    /// description for a Terraform Provider.
    /// 
    /// Because each RPC method is rather involved and potentially
    /// complicated on its own, each one is broken out into its own
    /// source file by using the _partial_ mechanism.
    /// </summary>
    public partial class PluginService : Provider.ProviderBase
    {
        // Needs to be `internal` to be accessible to TraceExecAttribute
        internal readonly ILogger _log;

        public PluginService(ILogger<PluginService> logger)
        {
            _log = logger;
        }
    }
}