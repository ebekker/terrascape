using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Terraform.Plugin.Attributes;
using Terraform.Plugin.Diagnostics;
using Terraform.Plugin.Skills;
using Terraform.Plugin.Util;
using Tfplugin5;

namespace Terraform.Plugin.Services
{
    public partial class PluginService
    {
        [TraceExec]
        public override async Task<ImportResourceState.Types.Response> ImportResourceState(
            ImportResourceState.Types.Request request, ServerCallContext context)
        {
            var response = new ImportResourceState.Types.Response();

            var providerType = _schemaResolver.PluginDetails.Provider;
            var resType = _schemaResolver.GetResourceSchemas()[request.TypeName].Type;

            if (providerType.HasImportResourceStateSkill(resType))
            {
                providerType.InvokeImportResourceStateSkill(
                    PluginProviderInstance,
                    resType,
                    writeInput: (inputType, input) => WriteInput(resType, request, inputType, input),
                    readResult: (resultType, result) => ReadResult(resType, resultType, result, request, response)
                    );
            }
            else
            {
                _log.LogWarning("provider does not handle importing state for resource [{type}]", resType);
            }

            return await Task.FromResult(response);
        }

        private void WriteInput(Type resType, ImportResourceState.Types.Request request, Type inputType, object input)
        {
            inputType.GetProperty(nameof(request.Id))
                .SetValue(input, request.Id);
        }

        private void ReadResult(Type resType, Type resultType, object result,
            ImportResourceState.Types.Request request,
            ImportResourceState.Types.Response response)
        {
            var diagnostics = ((TFDiagnostics)resultType
                .GetProperty(nameof(response.Diagnostics))
                .GetValue(result));
            if (diagnostics.Count() > 0)
                response.Diagnostics.Add(diagnostics.All());

            var importedEnum = resultType.GetProperty(nameof(response.ImportedResources))
                    .GetValue(result) as System.Collections.IEnumerable;

            if (importedEnum != null)
            {
                var importedType = typeof(TFImportedResource<>).MakeGenericType(resType);
                var stateProp = importedType
                    .GetProperty(nameof(TFImportedResource<object>.State));
                var privateProp = importedType
                    .GetProperty(nameof(TFImportedResource<object>.Private));

                var importedResult = new List<ImportResourceState.Types.ImportedResource>();
                foreach (var imported in importedEnum)
                {
                    importedResult.Add(new ImportResourceState.Types.ImportedResource
                    {
                        // We assume the outgoing is the same as incoming
                        TypeName = request.TypeName,
                        // Convert over private and schema-defined state
                        State = stateProp.GetValue(imported).MarshalToDynamicValue(resType),
                        Private = ByteString.CopyFrom((byte[])privateProp.GetValue(imported)),
                    });
                }
                response.ImportedResources.Add(importedResult);
            }
        }
    }
}
