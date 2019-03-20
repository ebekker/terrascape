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
    public class ApplyResourceChangeInput<T>
    {
        public ResourceChangeType ChangeType { get; set; }
        public T PriorState { get; set; }
        public T Config { get; set; }
        public byte[] PlannedPrivate { get; set; }
        public T PlannedState { get; set; }
    }

    public class ApplyResourceChangeResult<T> : IHasDiagnostics
    {
        public TFDiagnostics Diagnostics { get; set; }
        public T NewState { get; set; }
        public byte[] Private { get; set; }
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
            _log.LogDebug(">>>{method}>>>", nameof(ApplyResourceChange));
            _log.LogTrace($"->input[{nameof(request)}] = {{@request}}", request);
            _log.LogTrace($"->input[{nameof(context)}] = {{@context}}", context);

            try
            {
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

                    var config = DynamicValue.Unmarshal(resType, request.Config);
                    var priorState = DynamicValue.Unmarshal(resType, request.PriorState);
                    var plannedPrivate = request.PlannedPrivate.ToByteArray();
                    var plannedState = DynamicValue.Unmarshal(resType, request.PlannedState);

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

                    _log.LogDebug("Applying " + changeType.ToString().ToUpper());

                    invokeInputType.GetProperty(nameof(PlanResourceChangeInput<object>.ChangeType))
                        .SetValue(invokeInput, changeType);
                    invokeInputType.GetProperty(nameof(request.Config))
                        .SetValue(invokeInput, config);
                    invokeInputType.GetProperty(nameof(request.PriorState))
                        .SetValue(invokeInput, priorState);
                    invokeInputType.GetProperty(nameof(request.PlannedPrivate))
                        .SetValue(invokeInput, plannedPrivate);
                    invokeInputType.GetProperty(nameof(request.PlannedState))
                        .SetValue(invokeInput, plannedState);

                    // Invoke the functional method
                    var invokeMethod = invokeType.GetMethod(nameof(IHasApplyResourceChange<object>.ApplyChange));
                    var invokeResult = invokeMethod.Invoke(_providerInstance, new[] { invokeInput });
                    if (invokeResult == null)
                        throw new Exception("invocation result returned null");
                    if (!invokeResultType.IsAssignableFrom(invokeResult.GetType()))
                        throw new Exception("invocation result not of expected type or subclass");

                    // Deconstruct the result to response type
                    var diagnostics = ((TFDiagnostics)invokeResultType
                        .GetProperty(nameof(response.Diagnostics)).GetValue(invokeResult));
                    var newPrivate = invokeResultType
                        .GetProperty(nameof(response.Private)).GetValue(invokeResult);
                    var newState = invokeResultType
                        .GetProperty(nameof(response.NewState)).GetValue(invokeResult);

                    if (diagnostics.Count() > 0)
                        response.Diagnostics.Add(diagnostics.All());
                    if (newPrivate != null)
                        response.Private = ByteString.CopyFrom((byte[])newPrivate);
                    if (newState != null)
                        response.NewState = DynamicValue.Marshal(resType, newState);
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
