using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using HC.TFPlugin.Attributes;
using HC.TFPlugin.Diagnostics;
using Microsoft.Extensions.Logging;
using Tfplugin5;
using static Tfplugin5.AttributePath.Types;
using static Tfplugin5.AttributePath.Types.Step;

namespace HC.TFPlugin
{
    public class ConfigureInput
    {
        public string TerraformVersion { get; set; }
    }

    public class ConfigureResult : IHasDiagnostics
    {
        public TFDiagnostics Diagnostics { get; set; }
    }

    public interface IHasConfigure
    {
        ConfigureResult Configure(ConfigureInput input);
    }

    public partial class ProviderImpl
    {

        public override async Task<Tfplugin5.Configure.Types.Response> Configure(
            Tfplugin5.Configure.Types.Request request, ServerCallContext context)
        {
            _log.LogDebug(">>>{method}>>>", nameof(Configure));
            _log.LogTrace($"->input[{nameof(request)}] = {{@request}}", request);
            _log.LogTrace($"->input[{nameof(context)}] = {{@context}}", context);

            try
            {
                var response = new Tfplugin5.Configure.Types.Response();

                var plugin = SchemaHelper.GetPluginDetails(PluginAssembly);
                _providerInstance = DynamicValue.Unmarshal(plugin.Provider, request.Config);

                if (typeof(IHasConfigure).IsAssignableFrom(plugin.Provider))
                {
                    var invokeInput = new ConfigureInput
                    {
                        TerraformVersion = request.TerraformVersion,
                    };
                    
                    var invokeResult = (_providerInstance as IHasConfigure).Configure(invokeInput);
                    if (invokeResult == null)
                        throw new Exception("invocation result returned null");
                    
                    var diagnostics = invokeResult.Diagnostics;
                    if (diagnostics.Count() > 0)
                        response.Diagnostics.Add(diagnostics.All());
                }
                
                _log.LogTrace("<-result = {@response}", response);
                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "<!exception = ");
                throw;
            }
        }
    }
}
