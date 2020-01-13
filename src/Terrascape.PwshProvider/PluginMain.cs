using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsgPackSharp;
using Terraform.Plugin;
using Terraform.Plugin.Attributes;

[assembly:TFPlugin(provider: typeof(Terrascape.PwshProvider.PluginProvider))]

namespace Terrascape.PwshProvider
{
    class PluginMain
    {
        static async Task Main(string[] args)
        {
            var server = new TFPluginServer();

            server.Prepare(args);

            var log = server.Services.GetRequiredService<ILogger<PluginMain>>();
            log.LogInformation("TF Plugin Server prepared");

            var logFactory = server.Services.GetRequiredService<ILoggerFactory>();
            MsgPackSharp.Logging.Factory = new MPLogFactory(logFactory);

            log.LogInformation("Running Plugin Server...");
            await server.RunAsync();

            log.LogInformation("Plugin Server exited");

            // var rs = System.Management.Automation.Runspaces.Runspace.DefaultRunspace; 
            // var rs2 = System.Management.Automation.Runspaces.RunspaceFactory.CreateRunspace();
            // var init = System.Management.Automation.Runspaces.InitialSessionState.Create();

            // using var ps = PowerShell.Create();
            // ps.Runspace.SessionStateProxy.SetVariable("foo", "FOOVALUE");

            // if (args?.Length > 0)
            // {
            //     foreach (var a in args)
            //     {
            //         var result = ps.AddScript(a).Invoke();

            //         foreach (var e in ps.Streams.Error)
            //             Console.Error.WriteLine(e.ToString());

            //         foreach (var w in ps.Streams.Warning)
            //             Console.Error.WriteLine(w.ToString());

            //         foreach (var r in result)
            //             Console.WriteLine("Result: {0}",
            //                 (r ?? "(NULL)").ToString()); //JsonSerializer.Serialize());
                    
            //     }
            // }
        }
    }

    public class MPLogFactory : MsgPackSharp.ILogFactory
    {
        //private IServiceProvider _services;
        private ILoggerFactory _factory;

        // public MPLogFactory(IServiceProvider services)
        // {
        //     _services = services;
        // }
        public MPLogFactory(ILoggerFactory factory)
        {
            _factory = factory;
        }

        public ILog Get(string name) => new MPLog(_factory.CreateLogger(name));
    }

    public class MPLog : MsgPackSharp.ILog
    {
        private ILogger _log;

        public MPLog(ILogger log)
        {
            _log = log;
        }

        public void Dbg(string fmt, params object[] args) => _log.LogDebug(fmt, args);
        public void Err(string fmt, params object[] args) => _log.LogError(fmt, args);
        public void Ftl(string fmt, params object[] args) => _log.LogCritical(fmt, args);
        public void Inf(string fmt, params object[] args) => _log.LogInformation(fmt, args);
        public void Trc(string fmt, params object[] args) => _log.LogTrace(fmt, args);
        public void Wrn(string fmt, params object[] args) => _log.LogWarning(fmt, args);
    }
}
