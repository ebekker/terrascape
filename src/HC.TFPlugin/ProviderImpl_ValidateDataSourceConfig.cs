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
    public class ValidateDataSourceConfigInput<T>
    {
        public T Config { get; set; }
    }

    public class ValidateDataSourceConfigResult<T> : IHasDiagnostics
    {
        public TFDiagnostics Diagnostics { get; set; }
    }

    public interface IHasValidateDataSourceConfig<T>
    {
        ValidateDataSourceConfigResult<T> ValidateConfig(ValidateDataSourceConfigInput<T> input);
    }

    public partial class ProviderImpl
    {
        public override async Task<Tfplugin5.ValidateDataSourceConfig.Types.Response> ValidateDataSourceConfig(
            Tfplugin5.ValidateDataSourceConfig.Types.Request request, ServerCallContext context)
        {
            _log.LogDebug(">>>{method}>>>", nameof(ValidateDataSourceConfig));
            _log.LogTrace($"->input[{nameof(request)}] = {{@request}}", request);
            _log.LogTrace($"->input[{nameof(context)}] = {{@context}}", context);

            try
            {
                if (_ProviderInstance == null)
                    throw new Exception("provider instance was not configured previously");

                var response = new Tfplugin5.ValidateDataSourceConfig.Types.Response();

                var plugin = SchemaHelper.GetPluginDetails(PluginAssembly);
                var resType = plugin.DataSources.Where(x =>
                    request.TypeName == x.GetCustomAttribute<TFDataSourceAttribute>()?.Name).First();
                var invokeType = typeof(IHasValidateDataSourceConfig<>).MakeGenericType(resType);
                if (invokeType.IsAssignableFrom(plugin.Provider))
                {
                    var invokeInputType = typeof(ValidateDataSourceConfigInput<>).MakeGenericType(resType);
                    var invokeResultType = typeof(ValidateDataSourceConfigResult<>).MakeGenericType(resType);

                    // Construct and populate the input type instance from the request
                    var invokeInput = Activator.CreateInstance(invokeInputType);

                    invokeInputType.GetProperty(nameof(request.Config)).SetValue(invokeInput,
                        DynamicValue.Unmarshal(resType, request.Config));

                    // Invoke the functional method
                    var invokeMethod = invokeType.GetMethod(nameof(IHasValidateDataSourceConfig<object>.ValidateConfig));
                    var invokeResult = invokeMethod.Invoke(_ProviderInstance, new[] { invokeInput });
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