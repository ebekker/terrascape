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
    public class ApplyResourceChangeInput<T>
    {
        public T PriorState { get; set; }
        public T Config { get; set; }
        public byte[] PlannedPrivate { get; set; }
        public T PlannedState { get; set; }
    }

    public class ApplyResourceChangeResult<T>
    {
        public T NewState { get; set; }
        public byte[] Private { get; set; }
        public IEnumerable<ValuePath> RequiresReplace { get; set; }
    }

    public interface IHasApplyResourceChange<T>
    {
        ApplyResourceChangeResult<T> ApplyChange(ApplyResourceChangeInput<T> input);
    }

    public partial class ProviderImpl
    {

        public override async Task<Tfplugin5.ApplyResourceChange.Types.Response> ApplyResourceChange(
            Tfplugin5.ApplyResourceChange.Types.Request request, ServerCallContext context)
        {
try
{
            Dumper.Out.WriteLine("Called [ApplyResourceChange]");

            Dumper.Out.WriteLine($"    TypeName         = [{request.TypeName}]");
            Dumper.Out.WriteLine($"    Config           = [{Dump(request.Config)}]");
            Dumper.Out.WriteLine($"    PriorState       = [{Dump(request.PriorState)}]");
            Dumper.Out.WriteLine($"    PlannedPrivate   = [{request.PlannedPrivate?.ToStringUtf8()}]");
            Dumper.Out.WriteLine($"    PlannedState     = [{Dump(request.PlannedState)}]");

            var response = new Tfplugin5.ApplyResourceChange.Types.Response();

            var plugin = SchemaHelper.GetPluginDetails(PluginAssembly);
            var resType = plugin.Resources.Where(x =>
                request.TypeName == x.GetCustomAttribute<TFResourceAttribute>()?.Name).First();
            var invokeType = typeof(IHasApplyResourceChange<>).MakeGenericType(resType);
            if (invokeType.IsAssignableFrom(plugin.Provider))
            {
                var invokeInputType = typeof(ApplyResourceChangeInput<>).MakeGenericType(resType);
                var invokeResultType = typeof(ApplyResourceChangeResult<>).MakeGenericType(resType);

                // Construct and populate the input type instance from the request
                var invokeInput = Activator.CreateInstance(invokeInputType);

                invokeInputType.GetProperty(nameof(request.Config)).SetValue(invokeInput,
                    DVHelper.UnmarshalViaJson(resType, request.Config));

                invokeInputType.GetProperty(nameof(request.PriorState)).SetValue(invokeInput,
                    DVHelper.UnmarshalViaJson(resType, request.PriorState));

                invokeInputType.GetProperty(nameof(request.PlannedPrivate)).SetValue(invokeInput,
                    request.PlannedPrivate.ToByteArray());

                invokeInputType.GetProperty(nameof(request.PlannedState)).SetValue(invokeInput,
                    DVHelper.UnmarshalViaJson(resType, request.PlannedState));

                // Invoke the functional method
                var invokeMethod = invokeType.GetMethod(nameof(IHasApplyResourceChange<object>.ApplyChange));
                var invokeResult = invokeMethod.Invoke(_providerInstance, new[] { invokeInput });
                if (!invokeResultType.IsAssignableFrom(invokeResult.GetType()))
                    throw new Exception("invocation result not of expected type or subclass");

                // Deconstruct the result to response type
                var newPrivate = invokeResultType.GetProperty(nameof(response.Private))
                        .GetValue(invokeResult);
                if (newPrivate != null)
                    response.Private = ByteString.CopyFrom((byte[])newPrivate);

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
