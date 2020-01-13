using System;
using System.Threading.Tasks;

namespace HC.GoPlugin.KVExample
{
    class PluginMain
    {
        public static string listenHost = "localhost";
        public static int listenPort = 15_000;
        public static int appProtoVersion = 1;

        static async Task Main(string[] args)
        {
            var server = new PluginServer(listenHost, listenPort, appProtoVersion);

            server.Health.SetStatus("plugin", HealthStatus.Serving);
            server.Services.Add(Proto.KV.BindService(new KVImpl()));

            server.Start();
            await server.WriteHandshakeAsync();

            Console.WriteLine("Hit a key to exit...");
            Console.ReadKey();
            Console.WriteLine("...shutting down...");
            await server.ShutdownAsync();
        }
    }
}
