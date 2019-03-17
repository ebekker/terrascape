using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using HC.GoPlugin;
using Newtonsoft.Json;

namespace HC.TFPlugin
{
    public class TFPluginServer
    {
        public const string MinPortName = "PLUGIN_MIN_PORT";
        public const string MaxPortName = "PLUGIN_MAX_PORT";

        // https://github.com/hashicorp/terraform/blob/master/plugin/serve.go#L35
        public const string HandshakeMagicCookieName = "TF_PLUGIN_MAGIC_COOKIE";
        public const string HandshakeMagicCookieValue = "d602bf8f470bc67ca7faa0386276bbdd4330efaf76d1a219cb4d6991ca9872b2";

        // https://github.com/hashicorp/terraform/blob/v0.12.0-beta1/plugin/serve.go#L16
        //private static int _appProtoVersion = 4; // TF 0.11
        private static int _appProtoVersion = 5; // TF 0.12

        private static string _listenHost = "localhost";
        private static int _listenPort = 15_000;

        private static ITLSConfig _tls;

        public static async Task<PluginServer> BuildServer(Assembly pluginAssembly = null)
        {
            var magic = System.Environment.GetEnvironmentVariable(HandshakeMagicCookieName);
            if (HandshakeMagicCookieValue != magic)
                throw new Exception("plugin should only be invoked by host");
            
            if (!int.TryParse(Environment.GetEnvironmentVariable(MinPortName), out var minPort))
                minPort = _listenPort;

            if (!int.TryParse(Environment.GetEnvironmentVariable(MaxPortName), out var maxPort))
                maxPort = _listenPort + 100;

            _listenPort = FindFreePort(minPort, maxPort);
            if (_listenPort <= 0)
                throw new Exception("could not find a free listening port in the requested range");

            _tls = TLSConfigSimple.GenerateSelfSignedRSA();

            Dumper.Out.WriteLine($"listen ports: {minPort}-{maxPort} : {_listenPort}");

            var server = new PluginServer(_listenHost, _listenPort, _appProtoVersion, _tls);
            var provider = new ProviderImpl(pluginAssembly);

            server.Services.Add(ProviderImpl.BindService(provider));
            server.Health.SetStatus("plugin", HealthStatus.Serving);
            server.Start();
            await server.WriteHandshakeAsync();

            Console.WriteLine("Hit a key to exit...");
            Console.ReadKey();
            Console.WriteLine("...shutting down...");

            await server.ShutdownAsync();

            return server;
        }

        // This is somewhat kludgey as there is a race condition in between
        // finding (and binding to) a free port and then releasing it so that
        // it can be rebound to by the gRPC Server class, but we have limited
        // options because of the interface exposed by the gRPC Server class
        private static int FindFreePort(int min, int max)
        {
            if (max < min)
                return -1;

            for (var cur = min; cur < max; ++cur)
            {
                var l = new TcpListener(IPAddress.Loopback, cur);
                try
                {
                    l.Start();
                    l.Stop();
                    return cur;
                }
                catch (Exception)
                { }
            }

            return -1;
        }
    }
}