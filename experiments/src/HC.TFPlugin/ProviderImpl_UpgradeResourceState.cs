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
    public class UpgradeResourceStateInput<T>
    {
        public T CurrentState { get; set; }
    }

    public class UpgradeResourceStateResult<T> : IHasDiagnostics
    {
        public TFDiagnostics Diagnostics { get; set; }
        public T NewState { get; set; }
    }

    public interface IHasUpgradeResourceState<T>
    {
        UpgradeResourceStateResult<T> Read(UpgradeResourceStateInput<T> input);
    }

    public partial class ProviderImpl
    {
        public override async Task<Tfplugin5.UpgradeResourceState.Types.Response> UpgradeResourceState(
            Tfplugin5.UpgradeResourceState.Types.Request request, ServerCallContext context)
        {
            _log.LogDebug(">>>{method}>>>", nameof(UpgradeResourceState));
            _log.LogTrace($"->input[{nameof(request)}] = {{@request}}", request);
            _log.LogTrace($"->input[{nameof(context)}] = {{@context}}", context);

            try
            {
                var response = new Tfplugin5.UpgradeResourceState.Types.Response();

                // var plugin = SchemaHelper.GetPluginDetails(PluginAssembly);
                // var resType = plugin.Resources.Where(x =>
                //     request.TypeName == x.GetCustomAttribute<TFResourceAttribute>()?.Name).First();
                // var invokeType = typeof(IHasUpgradeResourceState<>).MakeGenericType(resType);
                // if (invokeType.IsAssignableFrom(plugin.Provider))
                // {
                //     var invokeInputType = typeof(UpgradeResourceStateInput<>).MakeGenericType(resType);
                //     var invokeResultType = typeof(UpgradeResourceStateResult<>).MakeGenericType(resType);

                //     // Construct and populate the input type instance from the request
                //     var invokeInput = Activator.CreateInstance(invokeInputType);

                //     invokeInputType.GetProperty(nameof(request.CurrentState)).SetValue(invokeInput,
                //         DVHelper.UnmarshalViaJson(resType, request.));

                //     // Invoke the functional method
                //     var invokeMethod = invokeType.GetMethod(nameof(IHasUpgradeResourceState<object>.Read));
                //     var invokeResult = invokeMethod.Invoke(_providerInstance, new[] { invokeInput });
                //     if (invokeResult == null)
                //         throw new Exception("invocation result returned null");
                //     if (!invokeResultType.IsAssignableFrom(invokeResult.GetType()))
                //         throw new Exception("invocation result not of expected type or subclass");

                //     // Deconstruct the result to response type
                //     var diagnostics = ((TFDiagnostics)invokeResultType.GetProperty(nameof(response.Diagnostics))
                //         .GetValue(invokeResult));
                //     if (diagnostics.Count > 0)
                //         response.Diagnostics.Add(diagnostics.All);

                //     var newState = invokeResultType.GetProperty(nameof(response.NewState))
                //             .GetValue(invokeResult);
                //     if (newState != null)
                //         response.NewState = DVHelper.MarshalViaJson(resType, newState);
                // }

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
