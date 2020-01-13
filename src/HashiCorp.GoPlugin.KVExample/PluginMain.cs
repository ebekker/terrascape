using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HashiCorp.GoPlugin.KVExample
{
    public class PluginMain
    {
        public static string ListenHost { get; set; } = "localhost";

        public static int ListenPort { get; set; } = 3000;

        public static async Task Main(string[] args)
        {
            var pluginHost = new PluginHost();

            // KV sample client doesn't support MTLS
            // pluginHost.PKI = PKI.SimplePKIDetails.GenerateRSA();

            pluginHost.PrepareHost(args);
            pluginHost.BuildHostApp();
            pluginHost.MapGrpcService<Services.KVService>();
            var log = pluginHost.Services.GetRequiredService<ILogger<PluginMain>>();
            log.LogInformation("Starting up Plugin Host...");
            var hostingTask = pluginHost.StartHosting();

            log.LogInformation("Capturing console cancel request");
            Console.CancelKeyPress += (o, e) => {
                log.LogInformation("Got CANCEL request");
                pluginHost.StopHosting();
                log.LogInformation("Initiated Plugin Stop");
            };

            Console.WriteLine("Running...");
            Console.WriteLine("Hit CTRL+C to exit.");

            _ = Task.Run(async () => {
                await Task.Delay(20 * 1000);
                log.LogInformation("Timedout hosting");
                pluginHost.StopHosting();
                log.LogInformation("Initiated Plugin Stop");
            });

            await hostingTask;
        }
    }
}