using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using HC.TFPlugin.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tfplugin5;
using static Tfplugin5.AttributePath.Types;
using static Tfplugin5.AttributePath.Types.Step;
using ProtoSchema = Tfplugin5.Schema;

namespace HC.TFPlugin
{
    public partial class ProviderImpl : Tfplugin5.Provider.ProviderBase
    {
        private ILogger _log = LogUtil.Create<ProviderImpl>();

        public object _providerInstance;

        public ProviderImpl(Assembly pluginAssembly = null)
        {
            PluginAssembly = pluginAssembly;
        }

        public Assembly PluginAssembly { get; }

        public override Task<Tfplugin5.GetProviderSchema.Types.Response> GetSchema(
            Tfplugin5.GetProviderSchema.Types.Request request, ServerCallContext context)
        {
            _log.LogDebug("Called [GetSchema]");

            try
            {
                var response = new Tfplugin5.GetProviderSchema.Types.Response();
                response.Provider = SchemaHelper.GetProviderSchema(PluginAssembly);
                response.DataSourceSchemas.Add(SchemaHelper.GetDataSourceSchemas());
                response.ResourceSchemas.Add(SchemaHelper.GetResourceSchemas());
                
                // _log.LogInformation($"  DataSources: [{response.DataSourceSchemas.Count}]");
                // _log.LogInformation($"  Resources: [{response.ResourceSchemas.Count}]");

                foreach (var rs in response.ResourceSchemas)
                {
                    _log.LogInformation($"    [{rs.Key}]=[{rs.Value.Version}][{string.Join(",", rs.Value.Block.Attributes)}]");
                }

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _log.LogInformation("ERROR: " + ex);
                throw;
            }
        }

        public override async Task<Tfplugin5.PrepareProviderConfig.Types.Response> PrepareProviderConfig(
            Tfplugin5.PrepareProviderConfig.Types.Request request, ServerCallContext context)
        {
            _log.LogInformation("Called [PrepareProviderConfig]");
            _log.LogInformation($"  * Config = [{Dump(request.Config)}]");
            // _log.LogInformation($"  request.Config = [{JsonConvert.SerializeObject(request.Config.Msgpack.ToStringUtf8(), Formatting.Indented)}]");

            var response = new Tfplugin5.PrepareProviderConfig.Types.Response();
            (_providerInstance, response.PreparedConfig) = ProviderHelper.PrepareProviderConfig(
                request.Config, PluginAssembly);

            return await Task.FromResult(response);
        }

        public override async Task<Tfplugin5.ValidateResourceTypeConfig.Types.Response> ValidateResourceTypeConfig(
            Tfplugin5.ValidateResourceTypeConfig.Types.Request request, ServerCallContext context)
        {
            _log.LogInformation("Called [ValidateResourceTypeConfig]");
            _log.LogInformation($"  * TypeName = [{request.TypeName}]");
            _log.LogInformation($"  * Config   = [{Dump(request.Config)}]");

            var response = new Tfplugin5.ValidateResourceTypeConfig.Types.Response();
            ProviderHelper.ValidateResourceTypeConfig(_providerInstance, request.TypeName,
                request.Config, PluginAssembly);

            return await Task.FromResult(response);
        }

        public override async Task<Tfplugin5.ValidateDataSourceConfig.Types.Response> ValidateDataSourceConfig(
            Tfplugin5.ValidateDataSourceConfig.Types.Request request, ServerCallContext context)
        {
            _log.LogInformation("Called [ValidateDataSourceConfig]");
            _log.LogInformation($"  * TypeName = [{request.TypeName}]");
            _log.LogInformation($"  * Config   = [{Dump(request.Config)}]");

            var response = new Tfplugin5.ValidateDataSourceConfig.Types.Response();

            return await Task.FromResult(response);
        }

        public override async Task<Tfplugin5.Configure.Types.Response> Configure(
            Tfplugin5.Configure.Types.Request request, ServerCallContext context)
        {
            _log.LogInformation("Called [Configure]");
            _log.LogInformation($"  * TerraformVersion = [{request.TerraformVersion}]");
            _log.LogInformation($"  * Config           = [{Dump(request.Config)}]");

            var response = new Tfplugin5.Configure.Types.Response();
            _providerInstance = ProviderHelper.Configure(request.Config, PluginAssembly);

            return await Task.FromResult(response);
        }

        public override async Task<Tfplugin5.UpgradeResourceState.Types.Response> UpgradeResourceState(
            Tfplugin5.UpgradeResourceState.Types.Request request, ServerCallContext context)
        {
            _log.LogInformation("Called [UpgradeResourceState]");

            var response = new Tfplugin5.UpgradeResourceState.Types.Response();

            return await Task.FromResult(response);
        }

        // public override async Task<Tfplugin5.ReadResource.Types.Response> ReadResource(
        //     Tfplugin5.ReadResource.Types.Request request, ServerCallContext context)
        // {
        //     _log.LogInformation("Called [ReadResource]");
        //     _log.LogInformation($"  * TypeName     = [{request.TypeName}]");
        //     _log.LogInformation($"  * CurrentState = [{Dump(request.CurrentState)}]");

        //     var response = new Tfplugin5.ReadResource.Types.Response();

        //     return await Task.FromResult(response);
        // }

        // public override async Task<Tfplugin5.ApplyResourceChange.Types.Response> ApplyResourceChange(
        //     Tfplugin5.ApplyResourceChange.Types.Request request, ServerCallContext context)
        // {
        //     _log.LogInformation("Called [ApplyResourceChange]");

        //     _log.LogInformation($"    TypeName         = [{request.TypeName}]");
        //     _log.LogInformation($"    Config           = [{Dump(request.Config)}]");
        //     _log.LogInformation($"    PriorState       = [{Dump(request.PriorState)}]");
        //     _log.LogInformation($"    PlannedPrivate   = [{request.PlannedPrivate?.ToStringUtf8()}]");
        //     _log.LogInformation($"    PlannedState     = [{Dump(request.PlannedState)}]");

        //     var response = new Tfplugin5.ApplyResourceChange.Types.Response();

        //     response.NewState = request.PlannedState;
        //     response.Private = request.PlannedPrivate;

        //     return await Task.FromResult(response);
        // }

        public override async Task<Tfplugin5.ImportResourceState.Types.Response> ImportResourceState(
            Tfplugin5.ImportResourceState.Types.Request request, ServerCallContext context)
        {
            _log.LogInformation("Called [ImportResourceState]");

            var response = new Tfplugin5.ImportResourceState.Types.Response();

            return await Task.FromResult(response);
        }

        public static string Dump(DynamicValue dv)
        {
            string json = null;
            if (dv.Json != null)
            {
                json = dv.Json.ToStringUtf8();
            }

            string msgpack = null;
            if (dv.Msgpack != null)
            {
                msgpack = dv.Msgpack.ToStringUtf8();
            }

            return JsonConvert.SerializeObject(new { json, msgpack });
        }

        public class SysInfoInput
        {
            public string inp1 { get; set; }
            public bool inp2 { get; set; }
            public int inp3 { get; set; }
        }

        public class SysInfoOutput : SysInfoInput
        {
            public string name { get; set;}

            public string version { get; set; }
        }

        public override async Task<Tfplugin5.ReadDataSource.Types.Response> ReadDataSource(
            Tfplugin5.ReadDataSource.Types.Request request, ServerCallContext context)
        {
            _log.LogInformation("Called [ReadDataSource]");
            _log.LogInformation($"    TypeName = [{request.TypeName}]");
            _log.LogInformation($"    Config   = [{Dump(request.Config)}]");

            var response = new Tfplugin5.ReadDataSource.Types.Response();

            try
            {

                if (request.TypeName == "lo_sys_info")
                {
                    //var inp = Cty.MsgpackHelper.Unmarshal<SysInfoInput>(request.Config);
                    var inp = DVHelper.Unmarshal<SysInfoInput>(request.Config);
                    _log.LogInformation("  Got Input:  " + JsonConvert.SerializeObject(inp));
                    var outp = new SysInfoOutput
                    {
                        inp1 = inp.inp1,
                        inp2 = inp.inp2,
                        inp3 = inp.inp3,
                        name = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                        version = System.Environment.OSVersion.VersionString,
                    };

                    // response.Diagnostics.Add(new Diagnostic
                    // {

                    // }.);

                    // response.State = Cty.MsgpackHelper.MarshalDynamicValue<object>(null);
                    // response.State = Cty.MsgpackHelper.MarshalDynamicValue(new Dictionary<string, string>()); //new DynamicValue();
                    // response.State = Cty.MsgpackHelper.MarshalDynamicValue(outp);
                    response.State = DVHelper.Marshal(outp);
                    //response.State = request.Config;

                    _log.LogInformation("  Output = " + JsonConvert.SerializeObject(outp));
                }
            }
            catch (Exception ex)
            {
                _log.LogInformation("Failed to compute response:" + ex.ToString());
            }

            // try
            // {
            //     _log.LogInformation($"  Got SysInfoInput = [{JsonConvert.SerializeObject(inp)}]");
            // }
            // catch (Exception ex)
            // {
            //     _log.LogInformation("Failed to get os_name:" + ex.ToString());
            // }

            _log.LogInformation("  Sending Reponse State: " + response.State);

            return await Task.FromResult(response);
        }

        public override async Task<Tfplugin5.Stop.Types.Response> Stop(
            Tfplugin5.Stop.Types.Request request, ServerCallContext context)
        {
            _log.LogInformation("Called [Stop]");

            var response = new Tfplugin5.Stop.Types.Response();

            return await Task.FromResult(response);
        }

        /// <summary>
        /// Creates a service definition that can be registered with the server.
        /// </summary>
        public static ServerServiceDefinition BindService(ProviderImpl impl) => Tfplugin5.Provider.BindService(impl);
    }
}
