using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Proto;

namespace HashiCorp.GoPlugin.KVExample.Services
{
    public class KVService : KV.KVBase
    {
        private static readonly Empty DefaultEmpty = new Empty();

        private ILogger _log;
        private PluginHost _ph;

        public KVService(ILogger<KVService> log, PluginHost ph)
        {
            _log = log;
            _ph = ph;

            _log.LogInformation("KV Service is ALIVE!");
        }

        public override Task<Empty> Put(Proto.PutRequest request,
            Grpc.Core.ServerCallContext context)
        {
            _log.LogInformation("Got Put with [{0}]=[{1}]", request.Key,
                request.Value.ToStringUtf8());
            
            return Task.FromResult(DefaultEmpty);
        }

        public override Task<GetResponse> Get(Proto.GetRequest request,
            Grpc.Core.ServerCallContext context)
        {
            _log.LogInformation("Got Get with [{0}]", request.Key);
            
            // Artificial delay...
            Thread.Sleep(15 * 1000);

            return Task.FromResult(new GetResponse
            {
                Value = ByteString.CopyFromUtf8(
                    $"{request.Key}=[{request.Key.ToUpper()}]"),
            });
        }
    }
}