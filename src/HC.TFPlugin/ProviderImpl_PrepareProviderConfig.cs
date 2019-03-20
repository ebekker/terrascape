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
    public class PrepareProviderConfigInput
    {
        // Empty class for now, but open for future expansion
        // Also preserves consistency in pattern of Provider impl handling
    }

    public class PrepareProviderConfigResult : IHasDiagnostics
    {
        public TFDiagnostics Diagnostics { get; set; }
    }

    /// <summary>
    /// PrepareProviderConfig allows the provider to validate the configuration
	/// values, and set or override any values with defaults.
    /// </summary>
    public interface IHasPrepareProviderConfig
    {
        PrepareProviderConfigResult PrepareConfig(PrepareProviderConfigInput input);
    }

    public partial class ProviderImpl
    {
        public override async Task<Tfplugin5.PrepareProviderConfig.Types.Response> PrepareProviderConfig(
            Tfplugin5.PrepareProviderConfig.Types.Request request, ServerCallContext context)
        {
            _log.LogDebug(">>>{method}>>>", nameof(PrepareProviderConfig));
            _log.LogTrace($"->input[{nameof(request)}] = {{@request}}", request);
            _log.LogTrace($"->input[{nameof(context)}] = {{@context}}", context);

            try
            {
                var response = new Tfplugin5.PrepareProviderConfig.Types.Response();

                // Default prepared config to incoming config
                response.PreparedConfig = request.Config;

                var plugin = SchemaHelper.GetPluginDetails(PluginAssembly);
                _providerInstance = DynamicValue.Unmarshal(plugin.Provider, request.Config);

                if (typeof(IHasPrepareProviderConfig).IsAssignableFrom(plugin.Provider))
                {
                    var invokeInput = new PrepareProviderConfigInput();

                    var invokeResult = (_providerInstance as IHasPrepareProviderConfig).PrepareConfig(invokeInput);
                    if (invokeResult == null)
                        throw new Exception("invocation result returned null");

                    var diagnostics = invokeResult.Diagnostics;
                    if (diagnostics.Count() > 0)
                        response.Diagnostics.Add(diagnostics.All());

                    response.PreparedConfig = DynamicValue.Marshal(plugin.Provider, _providerInstance);
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
