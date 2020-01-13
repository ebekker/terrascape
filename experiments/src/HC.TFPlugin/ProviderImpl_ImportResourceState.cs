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
using static Tfplugin5.ImportResourceState.Types;

namespace HC.TFPlugin
{
    public class ImportResourceStateInput<T>
    {
        public string Id { get; set; }
    }

    public class ImportResourceStateResult<T> : IHasDiagnostics
    {
        public TFDiagnostics Diagnostics { get; set; }

        public IEnumerable<ImportedResourceState<T>> ImportedResources { get; set; }
    }

    public class ImportedResourceState<T>
    {
        public byte[] Private { get; set; }
        public T State { get; set; }
    }

    public interface IHasImportResourceState<T>
    {
        ImportResourceStateResult<T> ImportResource(ImportResourceStateInput<T> input);
    }

    public partial class ProviderImpl
    {
        public override async Task<Tfplugin5.ImportResourceState.Types.Response> ImportResourceState(
            Tfplugin5.ImportResourceState.Types.Request request, ServerCallContext context)
        {
            _log.LogDebug(">>>{method}>>>", nameof(ImportResourceState));
            _log.LogTrace($"->input[{nameof(request)}] = {{@request}}", request);
            _log.LogTrace($"->input[{nameof(context)}] = {{@context}}", context);

            try
            {
                var response = new Tfplugin5.ImportResourceState.Types.Response();

                var plugin = SchemaHelper.GetPluginDetails(PluginAssembly);
                var resType = plugin.Resources.Where(x =>
                    request.TypeName == x.GetCustomAttribute<TFResourceAttribute>()?.Name).First();
                var invokeType = typeof(IHasImportResourceState<>).MakeGenericType(resType);
                if (invokeType.IsAssignableFrom(plugin.Provider))
                {
                    var invokeInputType = typeof(ImportResourceStateInput<>).MakeGenericType(resType);
                    var invokeResultType = typeof(ImportResourceStateResult<>).MakeGenericType(resType);

                    // Construct and populate the input type instance from the request
                    var invokeInput = Activator.CreateInstance(invokeInputType);

                    invokeInputType.GetProperty(nameof(request.Id)).SetValue(invokeInput, request.Id);

                    // Invoke the functional method
                    var invokeMethod = invokeType.GetMethod(nameof(IHasImportResourceState<object>.ImportResource));
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

                    var importedEnum = invokeResultType.GetProperty(nameof(response.ImportedResources))
                            .GetValue(invokeResult);

                    if (importedEnum != null)
                    {
                        var importedType = typeof(ImportedResourceState<>).MakeGenericType(resType);
                        var privateProp = importedType.GetProperty(nameof(ImportedResource.Private));
                        var stateProp = importedType.GetProperty(nameof(ImportedResource.State));

                        var importedResult = new List<ImportedResource>();
                        foreach (var imported in ((System.Collections.IEnumerable)importedEnum))
                        {
                            importedResult.Add(new ImportedResource
                            {
                                // We assume the outgoing is the same as incoming
                                TypeName = request.TypeName,
                                // Convert over private and schema-defined state
                                Private = ByteString.CopyFrom((byte[])privateProp.GetValue(imported)),
                                State = DynamicValue.Marshal(resType, stateProp.GetValue(imported)),
                            });
                        }
                        response.ImportedResources.Add(importedResult);
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
