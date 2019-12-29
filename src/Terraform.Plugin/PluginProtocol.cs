using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Terraform.Plugin
{
    public class PluginProtocol
    {
        private static readonly ILogger _log =
            PluginHost.LoggerFactory.CreateLogger<PluginProtocol>();

        public const string MagicCookieEnv = "TF_PLUGIN_MAGIC_COOKIE";
        public const string MagicCookieEnvValue = "d602bf8f470bc67ca7faa0386276bbdd4330efaf76d1a219cb4d6991ca9872b2";

        public const string MaxPortEnv = "PLUGIN_MAX_PORT";
        public const string MinPortEnv = "PLUGIN_MIN_PORT";
        public const string ProtoVersionsEnv = "PLUGIN_PROTOCOL_VERSIONS";
        public const string ProtoVersionsEnvValue = "5";

        public int MinPort { get; private set; } = -1;
        public int MaxPort { get; private set; } = -1;

        public void Resolve()
        {
            var magicCookie = Environment.GetEnvironmentVariable(MagicCookieEnv);
            _log.LogInformation("Got magic cookie: {0}", magicCookie);
            if (!(magicCookie?.Equals(MagicCookieEnvValue) ?? false))
            {
                _log.LogError("Failed to get required magic cookie");
                throw new Exception("Magic Cookie failure");
            }

            var protoVersions = Environment.GetEnvironmentVariable(ProtoVersionsEnv);
            _log.LogInformation("Got protocol versions: {0}", protoVersions);
            if (!(protoVersions?.Equals(ProtoVersionsEnvValue) ?? false))
            {
                _log.LogError("Failed to get required protocol versions");
                throw new Exception("Protocol Versions failure");
            }

            MinPort = GetPort(MinPortEnv, "Min Port");
            MaxPort = GetPort(MaxPortEnv, "Max Port");

            if (MinPort < 0)
                _log.LogWarning($"{MinPortEnv} is unspecified");
            else if (MaxPort < 0)
                _log.LogWarning($"{MaxPortEnv} is unspecified");
            if (MaxPort < MinPort)
                throw new Exception("Protocol Ports are reversed");

            static int GetPort(string env, string name)
            {
                var port = Environment.GetEnvironmentVariable(env);
                if (string.IsNullOrWhiteSpace(port))
                    return -1;

                if (!int.TryParse(port, out var portNum))
                    throw new Exception($"Invalid {name}");
                else if (portNum < 1 || portNum > short.MaxValue)
                    throw new Exception($"{name} Out of Range");
                else
                    return portNum;
            }
       }

       public const string Vers1 = "1";
       public const string Vers2 = "5";
       public const string NetProto = "tcp";
       public const string RpcProto = "grpc";

       public void Announce(string listenHost, int listenPort)
       {
           /** SAMPLE:
                {"@level":"debug","@message":"plugin address","@timestamp":"2019-12-27T15:46:22.895488-05:00","address":"127.0.0.1:10000","network":"tcp"}
                1|5|tcp|127.0.0.1:10000|grpc|
            **/

            var mesg = new Announcement
            {
                Address = $"{listenHost}:{listenPort}",
            };
            var json = JsonSerializer.Serialize(mesg);
            Console.Error.WriteLine(json);
            Console.WriteLine($"{Vers1}|{Vers2}|{NetProto}|{mesg.Address}|{RpcProto}|");
       }

       public class Announcement
       {
           [JsonPropertyName("@level")]
           public string Level { get; set; } = "debug";

           [JsonPropertyName("@message")]
           public string Message { get; set; } = "plugin address";

           [JsonPropertyName("@timestamp")]
           public string Timestamp { get; set; } = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffffffK");

           [JsonPropertyName("address")]
           public string Address { get; set; }

           [JsonPropertyName("network")]
           public string Network { get; set; } = "tcp";
       }
    }
}