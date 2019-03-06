using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Newtonsoft.Json;
using Proto;

namespace HC.GoPlugin.KVExample
{
    public class KVImpl : KV.KVBase
    {
        private Dictionary<string, string> _values;

        internal async Task<Dictionary<string, string>> GetValues()
        {
            if (_values == null)
            {
                try
                {
                    if (File.Exists("kv.json"))
                        _values = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                            await File.ReadAllTextAsync("kv.json"));
                }
                catch (Exception)
                { }
                if (_values == null)
                    _values = new Dictionary<string, string>();
            }

            return _values;
        }

        public async override Task<Empty> Put(PutRequest request, ServerCallContext context)
        {
            (await GetValues())[request.Key] = request.Value.ToStringUtf8();
            await File.WriteAllTextAsync("kv.json", JsonConvert.SerializeObject(_values));
            return await Task.FromResult(new Empty());
        }

        public async override Task<GetResponse> Get(GetRequest request, ServerCallContext context)
        {
            var valueBytes = ByteString.Empty;
            if ((await GetValues()).TryGetValue(request.Key, out var value))
                valueBytes = ByteString.CopyFromUtf8(value);
            return await Task.FromResult(new GetResponse { Value = valueBytes, });
        }
    }
}