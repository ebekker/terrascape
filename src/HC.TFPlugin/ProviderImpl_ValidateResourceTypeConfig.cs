using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using HC.TFPlugin.Attributes;
using HC.TFPlugin.Diagnostics;
using Microsoft.Extensions.Logging;
using Tfplugin5;

namespace HC.TFPlugin
{
    public class ValidateResourceTypeConfigInput<T>
    {
        public T Config { get; set; }
    }

    public class ValidateResourceTypeConfigResult<T> : IHasDiagnostics
    {
        public TFDiagnostics Diagnostics { get; set; }
    }

    public interface IHasValidateResourceTypeConfig<T>
    {
        ValidateResourceTypeConfigResult<T> ValidateConfig(ValidateResourceTypeConfigInput<T> input);
    }

    public partial class ProviderImpl
    {
        public override async Task<Tfplugin5.ValidateResourceTypeConfig.Types.Response> ValidateResourceTypeConfig(
            Tfplugin5.ValidateResourceTypeConfig.Types.Request request, ServerCallContext context)
        {
            _log.LogDebug(">>>{method}>>>", nameof(ValidateResourceTypeConfig));
            _log.LogTrace($"->input[{nameof(request)}] = {{@request}}", request);
            _log.LogTrace($"->input[{nameof(context)}] = {{@context}}", context);

            try
            {
                if (_providerInstance == null)
                    throw new Exception("provider instance was not configured previously");

                var response = new Tfplugin5.ValidateResourceTypeConfig.Types.Response();
                // ProviderHelper.ValidateResourceTypeConfig(_providerInstance, request.TypeName,
                //     request.Config, PluginAssembly);

                var plugin = SchemaHelper.GetPluginDetails(PluginAssembly);
                var resType = plugin.Resources.Where(x =>
                    request.TypeName == x.GetCustomAttribute<TFResourceAttribute>()?.Name).First();
                var invokeType = typeof(IHasValidateResourceTypeConfig<>).MakeGenericType(resType);
                if (invokeType.IsAssignableFrom(plugin.Provider))
                {
                    var invokeInputType = typeof(ValidateResourceTypeConfigInput<>).MakeGenericType(resType);
                    var invokeResultType = typeof(ValidateResourceTypeConfigResult<>).MakeGenericType(resType);

                    // Construct and populate the input type instance from the request
                    var invokeInput = Activator.CreateInstance(invokeInputType);

                    invokeInputType.GetProperty(nameof(request.Config)).SetValue(invokeInput,
                        DynamicValue.Unmarshal(resType, request.Config));

                    // Invoke the functional method
                    var invokeMethod = invokeType.GetMethod(nameof(IHasValidateResourceTypeConfig<object>.ValidateConfig));
                    var invokeResult = invokeMethod.Invoke(_providerInstance, new[] { invokeInput });
                    if (invokeResult == null)
                        throw new Exception("invocation result returned null");
                    if (!invokeResultType.IsAssignableFrom(invokeResult.GetType()))
                        throw new Exception("invocation result not of expected type or subclass");

                    // Deconstruct the result to response type
                    var diagnostics = ((TFDiagnostics)invokeResultType.GetProperty(nameof(response.Diagnostics))
                        .GetValue(invokeResult));
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