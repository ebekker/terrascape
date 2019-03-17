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
    public class PlanResourceChangeInput<T>
    {
        public T PriorState { get; set; }
        public T Config { get; set; }
        public byte[] PriorPrivate { get; set; }
        public T ProposedNewState { get; set; }
    }

    public class PlanResourceChangeResult<T>
    {
        public T PlannedState { get; set; }
        public byte[] PlannedPrivate { get; set; }
        public IEnumerable<ValuePath> RequiresReplace { get; set; }
    }

    public interface IHasPlanResourceChange<T>
    {
        PlanResourceChangeResult<T> PlanChange(PlanResourceChangeInput<T> input);
        // IEnumerable<ValuePath> PlanChange(T priorState, T config);
    }

    public partial class ProviderImpl
    {
        public override async Task<Tfplugin5.PlanResourceChange.Types.Response> PlanResourceChange(
            Tfplugin5.PlanResourceChange.Types.Request request, ServerCallContext context)
        {
try
{
            Dumper.Out.WriteLine("Called [PlanResourceChange]");

            Dumper.Out.WriteLine($"    TypeName         = [{request.TypeName}]");
            Dumper.Out.WriteLine($"    Config           = [{Dump(request.Config)}]");
            Dumper.Out.WriteLine($"    PriorState       = [{Dump(request.PriorState)}]");
            Dumper.Out.WriteLine($"    PriorPrivate     = [{request.PriorPrivate?.ToStringUtf8()}]");
            Dumper.Out.WriteLine($"    ProposedNewState = [{Dump(request.ProposedNewState)}]");

            var response = new Tfplugin5.PlanResourceChange.Types.Response();

            var plugin = SchemaHelper.GetPluginDetails(PluginAssembly);
            var resType = plugin.Resources.Where(x =>
                request.TypeName == x.GetCustomAttribute<TFResourceAttribute>()?.Name).First();
            var invokeType = typeof(IHasPlanResourceChange<>).MakeGenericType(resType);
            if (invokeType.IsAssignableFrom(plugin.Provider))
            {
                var invokeInputType = typeof(PlanResourceChangeInput<>).MakeGenericType(resType);
                var invokeResultType = typeof(PlanResourceChangeResult<>).MakeGenericType(resType);

                // Construct and populate the input type instance from the request
                var invokeInput = Activator.CreateInstance(invokeInputType);

                invokeInputType.GetProperty(nameof(request.Config)).SetValue(invokeInput,
                    DVHelper.UnmarshalViaJson(resType, request.Config));

                invokeInputType.GetProperty(nameof(request.PriorState)).SetValue(invokeInput,
                    DVHelper.UnmarshalViaJson(resType, request.PriorState));

                invokeInputType.GetProperty(nameof(request.PriorPrivate)).SetValue(invokeInput,
                    request.PriorPrivate.ToByteArray());

                invokeInputType.GetProperty(nameof(request.ProposedNewState)).SetValue(invokeInput,
                    DVHelper.UnmarshalViaJson(resType, request.ProposedNewState));

                // Invoke the functional method
                var invokeMethod = invokeType.GetMethod(nameof(IHasPlanResourceChange<object>.PlanChange));
                var invokeResult = invokeMethod.Invoke(_providerInstance, new[] { invokeInput });
                if (!invokeResultType.IsAssignableFrom(invokeResult.GetType()))
                    throw new Exception("invocation result not of expected type or subclass");

                // Deconstruct the result to response type
                var plannedPrivate = invokeResultType.GetProperty(nameof(response.PlannedPrivate))
                        .GetValue(invokeResult);
                if (plannedPrivate != null)
                    response.PlannedPrivate = ByteString.CopyFrom((byte[])plannedPrivate);

                var plannedState = invokeResultType.GetProperty(nameof(response.PlannedState))
                        .GetValue(invokeResult);
                if (plannedState != null)
                    response.PlannedState = DVHelper.MarshalViaJson(resType, plannedState);

                var replaceChanges = invokeResultType.GetProperty(nameof(response.RequiresReplace))
                            .GetValue(invokeResult);
                if (replaceChanges != null)
                {
                    // Translates our internal representation of ValuePath to AttributePath
                    response.RequiresReplace.Add(((IEnumerable<ValuePath>)replaceChanges).Select(vp =>
                    {
                        var ap = new AttributePath();
                        ap.Steps.Add(vp.Segments.Select(vps =>
                            vps.selector == SelectorOneofCase.AttributeName
                                ? new Step { AttributeName = (string)vps.arg }
                                : vps.selector == SelectorOneofCase.ElementKeyString
                                    ? new Step { ElementKeyString = (string)vps.arg }
                                    : new Step { ElementKeyInt = (long)vps.arg }
                        ));
                        return ap;
                    }));
                }
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
