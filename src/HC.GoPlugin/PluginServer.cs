using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Grpc.Core;
using HC.GoPlugin.Impl;
using static Grpc.Core.Server;

namespace HC.GoPlugin
{
    /// <summary>
    /// Encapsulates the logic and convenience interface to support hosting a plugin
    /// compatible and interoperable with model defined by the <see
    /// cref="https://github.com/hashicorp/go-plugin">go-plugin</see> project.
    /// </summary>
    /// <remarks>
    /// This server implementation only supports a network type of <c>TCP</c> and
    /// a connection protocol of <c>grpc</c>.
    /// <para>
    /// In this implementation we don't support dynamic generation of TLS certificates
    /// if that is needed by the plugin consuming (client) process.  If TLS support is
    /// required (aka mutual-TLS or MTLS), then the server private key and certificate
    /// should be generated externally and made available by providing details in a
    /// <see cref="TLSConfig" /> instance.
    /// </para>
    /// </remarks>
    public class PluginServer
    {
        // https://github.com/hashicorp/go-plugin/blob/master/docs/guide-plugin-write-non-go.md#4-output-handshake-information
        public const int CoreProtocolVersion = 1;
        public const string NetworkType = "tcp";
        public const string ConnectionProtocol = "grpc";

        private Server _server;
        private ServerCredentials _serverCreds;
        private ServerPort _serverPort;
        private HealthServiceImpl _health;
        private string _ServerCertificate;
        private string _HandshakeInfo;

        public PluginServer(string listeningHost, int listeningPort, int appProtoVersion,
            ITLSConfig tlsConfig = null)
        {
            ListeningHost = listeningHost;
            ListeningPort = listeningPort;
            AppProtocolVersion = appProtoVersion;

            _server = new Server();
            _health = new HealthServiceImpl();
            _serverCreds = tlsConfig == null
                ? ServerCredentials.Insecure
                : TLSConfig.ToCredentials(tlsConfig);

            _serverPort = new ServerPort(ListeningHost, ListeningPort, _serverCreds);
            Server.Ports.Add(_serverPort);
            Server.Services.Add(Grpc.Health.V1.Health.BindService(_health));

            // Based on:
            //  https://github.com/hashicorp/go-plugin/blob/f444068e8f5a19853177f7aa0aea7e7d95b5b528/server.go#L257
            //  https://github.com/hashicorp/go-plugin/blob/f444068e8f5a19853177f7aa0aea7e7d95b5b528/server.go#L327
            if (tlsConfig != null)
            {
                _ServerCertificate = Convert.ToBase64String(tlsConfig.ServerCertRaw);
                _HandshakeInfo = string.Join("|",
                    CoreProtocolVersion,
                    AppProtocolVersion,
                    NetworkType,
                    NetworkAddres,
                    ConnectionProtocol,
                    _ServerCertificate
                );
            }
            else
            {
                _HandshakeInfo = string.Join("|",
                    CoreProtocolVersion,
                    AppProtocolVersion,
                    NetworkType,
                    NetworkAddres,
                    ConnectionProtocol
                );
            }
        }

        public string ListeningHost { get; }

        public int ListeningPort { get; }

        public int AppProtocolVersion { get; }

        public Server Server => _server;

        public ServiceDefinitionCollection Services => _server.Services;

        public IHealthService Health => _health;

        public string NetworkAddres => $"{ListeningHost}:{ListeningPort}";

        public string ServerCertificate => _ServerCertificate;

        public string HandshakeInfo => _HandshakeInfo;

        public void Start() => Server.Start();

        public async Task WriteHandshakeAsync() => await Console.Out.WriteAsync($"{HandshakeInfo}\n");

        public async Task ShutdownAsync() => await Server.ShutdownAsync();
    }
}
