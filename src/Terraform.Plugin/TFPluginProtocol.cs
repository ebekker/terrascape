using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Terraform.Plugin
{
    public class TFPluginProtocol
    {
        private static readonly ILogger _log =
            TFPluginServer.LoggerFactory.CreateLogger<TFPluginProtocol>();

        public const string Terraform011AppProtocolVersion = "4";
        public const string Terraform012AppProtocolVersion = "5";

        public const string MinPortEnv = "PLUGIN_MIN_PORT";
        public const string MaxPortEnv = "PLUGIN_MAX_PORT";

        // https://github.com/hashicorp/terraform/blob/master/plugin/serve.go#L35
        public const string MagicCookieEnv = "TF_PLUGIN_MAGIC_COOKIE";
        public const string MagicCookieEnvValue = "d602bf8f470bc67ca7faa0386276bbdd4330efaf76d1a219cb4d6991ca9872b2";

        public const string ProtoVersionsEnv = "PLUGIN_PROTOCOL_VERSIONS";
        public const string ProtoVersionsEnvValue = Terraform012AppProtocolVersion;

        public string AppProtocolVersion { get; } = Terraform012AppProtocolVersion;

        public int MinPort { get; private set; } = -1;
        public int MaxPort { get; private set; } = -1;

        public bool MagicCookieFound { get; }

        public bool ProtocolVersionMatched { get; }

        public bool PrepareHandshake()
        {
            var magicCookie = Environment.GetEnvironmentVariable(MagicCookieEnv);
            _log.LogDebug("Got magic cookie: {0}", magicCookie);
            if (!(magicCookie?.Equals(MagicCookieEnvValue) ?? false))
            {
                _log.LogError("Failed to get required magic cookie");
                return false;
            }

            var protoVersions = Environment.GetEnvironmentVariable(ProtoVersionsEnv);
            _log.LogDebug("Got protocol versions: {0}", protoVersions);
            if (!(protoVersions?.Equals(ProtoVersionsEnvValue) ?? false))
            {
                _log.LogError("Failed to get required protocol versions");
                return false;
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

            return true;
       }

        // public void CompleteHandshake(string listenHost, int listenPort, IServerPKIDetails pki = null)
        // {
        //     var address = $"{listenHost}:{listenPort}";
        //     var message = $"{CoreProtocolVersion}|{AppProtocolVersion}|{NetworkType}|{address}|{ConnectionProtocol}";

        //     if (pki != null)
        //     {
        //         var cert = pki.ToCertificate();
        //         var certEncoded = Convert.ToBase64String(cert.RawData,
        //             Base64FormattingOptions.None);
        //         message += $"|{certEncoded}";
        //     }

        //     Console.WriteLine(message);
        // }
    }
}