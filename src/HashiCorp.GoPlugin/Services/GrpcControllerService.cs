using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin;

namespace HashiCorp.GoPlugin.Services
{
    public class GrpcControllerService : Plugin.GRPCController.GRPCControllerBase
    {
        public static readonly Empty DefaultEmpty = new Empty();

        private ILogger _log;
        private PluginHost _pluginHost;

        public Action OnShutdown { get; set; }

        public GrpcControllerService(ILogger<GrpcControllerService> log, PluginHost pluginHost)
        {
            _log = log;
            _pluginHost = pluginHost;
            _log.LogDebug("GRPC Controller service constructed and ready");
        }        

        public override Task<Empty> Shutdown(Plugin.Empty request, Grpc.Core.ServerCallContext context)
        {
            _log.LogInformation("Got request for Shutdown, initiating...");

            if (OnShutdown != null)
            {
                _log.LogInformation("Custom handler found, invoking");
            }
            else
            {
                _log.LogInformation("Invoking default behavior, stopping Plugin Host");
                _pluginHost.StopHosting();
            }

            return Task.FromResult(DefaultEmpty);
        }        
    }
}