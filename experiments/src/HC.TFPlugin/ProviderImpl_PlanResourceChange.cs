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
    public enum ResourceChangeType
    {
        Unknown = 0,
        Create = 1,
        Update = 2,
        Delete = 3,
    }

    public class PlanResourceChangeInput<T>
    {
        public ResourceChangeType ChangeType { get; set; }
        public T PriorState { get; set; }
        public T Config { get; set; }
        public byte[] PriorPrivate { get; set; }
        public T ProposedNewState { get; set; }
    }

    public class PlanResourceChangeResult<T> : IHasDiagnostics
    {
        public TFDiagnostics Diagnostics { get; set; }
        public T PlannedState { get; set; }
        public byte[] PlannedPrivate { get; set; }
        public IEnumerable<TFSteps> RequiresReplace { get; set; }
    }

    public interface IHasPlanResourceChange<T>
    {
        PlanResourceChangeResult<T> PlanChange(PlanResourceChangeInput<T> input);
    }

    public partial class ProviderImpl
    {
        public override async Task<Tfplugin5.PlanResourceChange.Types.Response> PlanResourceChange(
            Tfplugin5.PlanResourceChange.Types.Request request, ServerCallContext context)
        {
            _log.LogDebug(">>>{method}>>>", nameof(PlanResourceChange));
            _log.LogTrace($"->input[{nameof(request)}] = {{@request}}", request);
            _log.LogTrace($"->input[{nameof(context)}] = {{@context}}", context);

            try
            {
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

                    var config = DynamicValue.Unmarshal(resType, request.Config);
                    var priorState = DynamicValue.Unmarshal(resType, request.PriorState);
                    var priorPrivate = request.PriorPrivate.ToByteArray();
                    var proposedNewState = DynamicValue.Unmarshal(resType, request.ProposedNewState);

                    var changeType = ResourceChangeType.Unknown;
                    if (priorState != null)
                        if (config == null)
                            changeType = ResourceChangeType.Delete;
                        else
                            changeType = ResourceChangeType.Update;
                    else
                        if (config != null)
                            changeType = ResourceChangeType.Create;
                        else
                            _log.LogWarning("Planning NULL -> NULL : You Should Never See This!");

                    _log.LogDebug("Planning " + changeType.ToString().ToUpper());

                    invokeInputType.GetProperty(nameof(PlanResourceChangeInput<object>.ChangeType))
                        .SetValue(invokeInput, changeType);
                    invokeInputType.GetProperty(nameof(request.Config))
                        .SetValue(invokeInput, config);
                    invokeInputType.GetProperty(nameof(request.PriorState))
                        .SetValue(invokeInput, priorState);
                    invokeInputType.GetProperty(nameof(request.PriorPrivate))
                        .SetValue(invokeInput, priorPrivate);
                    invokeInputType.GetProperty(nameof(request.ProposedNewState))
                        .SetValue(invokeInput, proposedNewState);

                    // Invoke the functional method
                    var invokeMethod = invokeType.GetMethod(nameof(IHasPlanResourceChange<object>.PlanChange));
                    var invokeResult = invokeMethod.Invoke(_ProviderInstance, new[] { invokeInput });
                    if (invokeResult == null)
                        throw new Exception("invocation result returned null");
                    if (!invokeResultType.IsAssignableFrom(invokeResult.GetType()))
                        throw new Exception("invocation result not of expected type or subclass");

                    // Deconstruct the result to response type
                    var diagnostics = ((TFDiagnostics)invokeResultType
                        .GetProperty(nameof(response.Diagnostics)) .GetValue(invokeResult));
                    var plannedPrivate = invokeResultType
                        .GetProperty(nameof(response.PlannedPrivate)).GetValue(invokeResult);
                    var plannedState = invokeResultType
                        .GetProperty(nameof(response.PlannedState)).GetValue(invokeResult);
                    var requiresReplace = invokeResultType
                        .GetProperty(nameof(response.RequiresReplace)).GetValue(invokeResult);

                    if (diagnostics.Count() > 0)
                        response.Diagnostics.Add(diagnostics.All());
                    if (plannedPrivate != null)
                        response.PlannedPrivate = ByteString.CopyFrom((byte[])plannedPrivate);
                    if (plannedState != null)
                        response.PlannedState = DynamicValue.Marshal(resType, plannedState);
                    

                    if (requiresReplace != null)
                    {
                        // Translates our internal representation of ValuePath to AttributePath
                        var paths = (IEnumerable<TFSteps>)requiresReplace;
                        response.RequiresReplace.Add(TFAttributePaths.ToPaths(paths));
                    }
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
