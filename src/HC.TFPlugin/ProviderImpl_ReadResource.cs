using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using HC.TFPlugin.Attributes;
using Tfplugin5;
using static Tfplugin5.AttributePath.Types;
using static Tfplugin5.AttributePath.Types.Step;

namespace HC.TFPlugin
{
    public class ReadResourceInput<T>
    {
        public T CurrentState { get; set; }
    }

    public class ReadResourceResult<T>
    {
        public T NewState { get; set; }
    }

    public interface IHasReadResource<T>
    {
        ReadResourceResult<T> Read(ReadResourceInput<T> input);
    }

    public partial class ProviderImpl
    {
        public override async Task<Tfplugin5.ReadResource.Types.Response> ReadResource(
            Tfplugin5.ReadResource.Types.Request request, ServerCallContext context)
        {
try
{
            Dumper.Out.WriteLine("Called [ReadResource]");
            Dumper.Out.WriteLine($"  * TypeName     = [{request.TypeName}]");
            Dumper.Out.WriteLine($"  * CurrentState = [{Dump(request.CurrentState)}]");

            var response = new Tfplugin5.ReadResource.Types.Response();

            var plugin = SchemaHelper.GetPluginDetails(PluginAssembly);
            var resType = plugin.Resources.Where(x =>
                request.TypeName == x.GetCustomAttribute<TFResourceAttribute>()?.Name).First();
            var invokeType = typeof(IHasReadResource<>).MakeGenericType(resType);
            if (invokeType.IsAssignableFrom(plugin.Provider))
            {
                var invokeInputType = typeof(ReadResourceInput<>).MakeGenericType(resType);
                var invokeResultType = typeof(ReadResourceResult<>).MakeGenericType(resType);

                // Construct and populate the input type instance from the request
                var invokeInput = Activator.CreateInstance(invokeInputType);

                invokeInputType.GetProperty(nameof(request.CurrentState)).SetValue(invokeInput,
                    DVHelper.UnmarshalViaJson(resType, request.CurrentState));

                // Invoke the functional method
                var invokeMethod = invokeType.GetMethod(nameof(IHasReadResource<object>.Read));
                var invokeResult = invokeMethod.Invoke(_providerInstance, new[] { invokeInput });
                if (!invokeResultType.IsAssignableFrom(invokeResult.GetType()))
                    throw new Exception("invocation result not of expected type or subclass");

                // Deconstruct the result to response type
                var newState = invokeResultType.GetProperty(nameof(response.NewState))
                        .GetValue(invokeResult);
                if (newState != null)
                    response.NewState = DVHelper.MarshalViaJson(resType, newState);
            }

            return await Task.FromResult(response);
}
catch (Exception ex)
{
    Dumper.Out.WriteLine("ERROR: " + ex);
    throw;
}
        }
    }
}
