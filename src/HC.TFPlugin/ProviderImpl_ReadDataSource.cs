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
    public class ReadDataSourceInput<T>
    {
        public T Config { get; set; }
    }

    public class ReadDataSourceResult<T> : IHasDiagnostics
    {
        public TFDiagnostics Diagnostics { get; set; }
        public T State { get; set; }
    }

    public interface IHasReadDataSource<T>
    {
        ReadDataSourceResult<T> Read(ReadDataSourceInput<T> input);
    }

    public partial class ProviderImpl
    {
        public override async Task<Tfplugin5.ReadDataSource.Types.Response> ReadDataSource(
            Tfplugin5.ReadDataSource.Types.Request request, ServerCallContext context)
        {
            _log.LogDebug(">>>{method}>>>", nameof(ReadDataSource));
            _log.LogTrace($"->input[{nameof(request)}] = {{@request}}", request);
            _log.LogTrace($"->input[{nameof(context)}] = {{@context}}", context);

            try
            {
                var response = new Tfplugin5.ReadDataSource.Types.Response();

                var plugin = SchemaHelper.GetPluginDetails(PluginAssembly);
                var resType = plugin.DataSources.Where(x =>
                    request.TypeName == x.GetCustomAttribute<TFDataSourceAttribute>()?.Name).First();
                var invokeType = typeof(IHasReadDataSource<>).MakeGenericType(resType);
                if (invokeType.IsAssignableFrom(plugin.Provider))
                {
                    var invokeInputType = typeof(ReadDataSourceInput<>).MakeGenericType(resType);
                    var invokeResultType = typeof(ReadDataSourceResult<>).MakeGenericType(resType);

                    // Construct and populate the input type instance from the request
                    var invokeInput = Activator.CreateInstance(invokeInputType);

                    invokeInputType.GetProperty(nameof(request.Config)).SetValue(invokeInput,
                        DynamicValue.Unmarshal(resType, request.Config));

                    // Invoke the functional method
                    var invokeMethod = invokeType.GetMethod(nameof(IHasReadDataSource<object>.Read));
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

                    var newState = invokeResultType.GetProperty(nameof(response.State))
                            .GetValue(invokeResult);
                    if (newState != null)
                        response.State = DynamicValue.Marshal(resType, newState);
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
