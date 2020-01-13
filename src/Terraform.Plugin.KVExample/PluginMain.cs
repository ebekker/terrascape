using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terraform.Plugin.Attributes;

[assembly:TFPlugin(provider: typeof(Terraform.Plugin.KVExample.PluginProvider))]

namespace Terraform.Plugin.KVExample
{
    class PluginMain
    {
        static async Task Main(string[] args)
        {
            var server = new TFPluginServer();

            server.Prepare(args);

            var log = server.Services.GetRequiredService<ILogger<PluginMain>>();
            log.LogInformation("TF Plugin Server prepared");

            log.LogInformation("Running Plugin Server...");
            await server.RunAsync();

            log.LogInformation("Plugin Server exited");
        }
    }
}
