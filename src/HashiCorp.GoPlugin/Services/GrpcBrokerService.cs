using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin;

namespace HashiCorp.GoPlugin.Services
{
    public class GrpcBrokerService : Plugin.GRPCBroker.GRPCBrokerBase
    {
        private readonly ILogger _log;
        private readonly PluginHost _pluginHost;

        public GrpcBrokerService(ILogger<GrpcBrokerService> log, PluginHost pluginHost)
        {
            _log = log;
            _pluginHost = pluginHost;
            _log.LogDebug("GRPC Broker service constructed and ready");
        }

        public override async Task StartStream(
            Grpc.Core.IAsyncStreamReader<Plugin.ConnInfo> requestStream,
            Grpc.Core.IServerStreamWriter<Plugin.ConnInfo> responseStream,
            Grpc.Core.ServerCallContext context)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(
                _pluginHost.StopHostingToken, context.CancellationToken);

            _log.LogInformation("Starting streaming...");
            var ct = cts.Token;

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    while (await requestStream.MoveNext(ct))
                    {
                        var connInfo = requestStream.Current;
                        _log.LogWarning("Not sure what to do -- got connection info:  {@conninfo}", connInfo);
                        await responseStream.WriteAsync(connInfo);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _log.LogWarning("Operation has been canceled");
            }

            _log.LogInformation("Streaming completed");
        }
    }
}