using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Infrastructure;
using Terraform.Plugin;
using Terraform.Plugin.Attributes;
using Terraform.Plugin.Diagnostics;
using Terraform.Plugin.Skills;

namespace Terrascape.PwshProvider
{
    [TFDataSource("pwsh_dsc_resource")]
    public class DscResDataSource : IDscRes
    {
        [TFArgument("module_name", Required = true)]
        public string ModuleName { get; set; }

        [TFArgument("module_version", Required = true)]
        public string ModuleVersion { get; set; }

        [TFArgument("type_name", Required = true)]
        public string TypeName { get; set; }

        [TFArgument("properties")]
        public Dictionary<string, string> Properties { get; set; }

        [TFArgument("computer_name", Optional = true)]
        public string ComputerName { get; set; }

        [TFComputed("results")]
        public Dictionary<string, string> Results { get; set; }

        [TFComputed("in_desired_state")]
        public bool? InDesiredState { get; set; }
    }

    public interface IDscRes
    {
        string ModuleName { get; }
        string ModuleVersion { get; }
        string TypeName { get; }
        Dictionary<string, string> Properties { get; }
        string ComputerName { get; }
        Dictionary<string, string> Results { get; set; }
        bool? InDesiredState { get; set; }
    }

    public partial class PluginProvider :
        IDataSourceProvider<DscResDataSource>
    {

        public HasValidateDataSourceConfig.Result<DscResDataSource> ValidateConfig(
            HasValidateDataSourceConfig.Input<DscResDataSource> input)
        {
            var result = new HasValidateDataSourceConfig.Result<DscResDataSource>();

            DscResValidateConfig(result, input.Config);

            return result;
        }

        public HasReadDataSource.Result<DscResDataSource> Read(
            HasReadDataSource.Input<DscResDataSource> input)
        {
            var result = new HasReadDataSource.Result<DscResDataSource>();

            result.State = (DscResDataSource)DscResReadConfig(result, input.Config);

            return result;
        }

        private object _dscSync = new object();

        private void DscResValidateConfig(IHasDiagnostics result, IDscRes config)
        {
            using var ses = CimSession.Create(config.ComputerName ?? DscRes.DefaultCimHostname);
            using var cls = ses.GetClass(DscRes.DscCimNamespace, config.TypeName);

            _log.LogDebug("Got CIM Class: {0}", config.TypeName);
            foreach (var p in cls.CimClassProperties)
            {
                _log.LogDebug("  * {0}: {1}; {2}; {3}; {4}",
                    p.Name,
                    p.CimType,
                    p.Flags,
                    p.ReferenceClassName,
                    string.Join(",", p.Qualifiers?.Select(q => q.Name)));
            }
            if (cls.CimClassProperties.Count == 0)
                result.AddError("Failed to get CIM Class properties");


            // First check for required params
            foreach (var p in cls.CimClassProperties)
            {
                if (p.Qualifiers.Any(q => q.Name == "key"))
                {
                    if (!config.Properties.ContainsKey(p.Name))
                    {
                        result.AddError($"missing mandatory DSC resource property: [{p.Name}]");
                    }
                }
            }

            // Check for existence, writability and value compatibility
            foreach (var c in config.Properties)
            {
                var p = cls.CimClassProperties.FirstOrDefault(p =>
                    p.Name.Equals(c.Key, StringComparison.OrdinalIgnoreCase));
                if (p == null)
                {
                    result.AddError($"unknown DSC resource property [{c.Key}]");
                    continue;
                }

                if (!p.Qualifiers.Any(q => q.Name == "write" || q.Name == "key"))
                {
                    result.AddError($"cannot set readonly DSC resource property [{p.Name}]");
                    continue;
                }

                if (p.CimType == CimType.Unknown)
                {
                    result.AddError($"cannot convert DSC resource property [{p.Name}] to [{p.CimType}]");
                    continue;
                }
                if (!DscRes.CanConvertTo(c.Value, p.CimType))
                {
                    result.AddError($"cannot convert DSC resource property [{p.Name}] to [{p.CimType}]");
                    continue;
                }

                if (p.Qualifiers.FirstOrDefault(q => q.Name == "ValueMap") is CimQualifier q)
                {
                    var validValues = (IEnumerable<string>)q.Value;
                    if (p.CimType == CimType.String)
                    {
                        if (!validValues.Contains(c.Value))
                        {
                            result.AddError($"invalid DSC resource property [{p.Name}]"
                                + $" - must be one of [{string.Join(",", validValues)}]");
                        }
                    }
                    else if (p.CimType == CimType.StringArray)
                    {
                        var canConvert = DscRes.TryConvertToArray<string>(c.Value, out var values);
                        if (values.Any(v => !validValues.Contains(v)))
                        {
                            result.AddError($"invalid DSC resource property [{p.Name}]"
                                + $" - must restrict to [{string.Join(",", validValues)}]");
                        }
                    }
                    else
                    {
                        result.AddError($"invalid DSC resource property [{p.Name}]"
                            + $" - don't know how to match {p.CimType} to restricted values");
                    }
                    continue;
                }
            }            
        }

        private bool DscResTestConfig(IHasDiagnostics result, IDscRes config)
        {
            using var ses = CimSession.Create(config.ComputerName ?? DscRes.DefaultCimHostname);
            using var cls = ses.GetClass(DscRes.DscCimNamespace, config.TypeName);
            using var cim = new CimInstance(DscRes.LcmCimClassName, DscRes.DscCimNamespace);

            var mofBody = DscRes.BuildMof(config, cls);
            _log.LogDebug("Generated MOF:\n{0}", mofBody);
            var mofBodyBytes = DscRes.ToUint8(mofBody);
            var methodParams = new CimMethodParametersCollection
            {
                CimMethodParameter.Create("ModuleName", config.ModuleName, CimFlags.None),
                CimMethodParameter.Create("ResourceType", config.TypeName, CimFlags.None),
                CimMethodParameter.Create("resourceProperty", mofBodyBytes, CimFlags.None),
            };

            CimMethodResult methodResult;
            
            lock (_dscSync) {
                methodResult = ses.InvokeMethod(cim, "ResourceTest", methodParams);
            }
            return (bool)methodResult.OutParameters["InDesiredState"].Value;
        }

        private IDscRes DscResReadConfig(IHasDiagnostics result, IDscRes config)
        {
            using var ses = CimSession.Create(config.ComputerName ?? DscRes.DefaultCimHostname);
            using var cls = ses.GetClass(DscRes.DscCimNamespace, config.TypeName);
            using var cim = new CimInstance(DscRes.LcmCimClassName, DscRes.DscCimNamespace);

            var mofBody = DscRes.BuildMof(config, cls);
            _log.LogDebug("Generated MOF:\n{0}", mofBody);
            var mofBodyBytes = DscRes.ToUint8(mofBody);
            var methodParams = new CimMethodParametersCollection
            {
                CimMethodParameter.Create("ModuleName", config.ModuleName, CimFlags.None),
                CimMethodParameter.Create("ResourceType", config.TypeName, CimFlags.None),
                CimMethodParameter.Create("resourceProperty", mofBodyBytes, CimFlags.None),
            };

            CimMethodResult methodResult;

            lock (_dscSync) {
                methodResult  = ses.InvokeMethod(cim, "ResourceGet", methodParams);
            }
            var dscResConfigs = (CimInstance)methodResult.OutParameters["configurations"].Value;
            
            var state = config;
            state.Results = new Dictionary<string, string>();
            foreach (var p in dscResConfigs.CimInstanceProperties)
            {
                state.Results[p.Name] = p.Value?.ToString() ?? string.Empty;
            }

            lock (_dscSync) {
                methodResult = ses.InvokeMethod(cim, "ResourceTest", methodParams);
            }
            state.InDesiredState = (bool)methodResult.OutParameters["InDesiredState"].Value;

            return state;
        }

        private IDscRes DscResApplyConfig(IHasDiagnostics result, IDscRes config,
            out bool rebootRequired)
        {
            using var ses = CimSession.Create(config.ComputerName ?? DscRes.DefaultCimHostname);
            using var cls = ses.GetClass(DscRes.DscCimNamespace, config.TypeName);
            using var cim = new CimInstance(DscRes.LcmCimClassName, DscRes.DscCimNamespace);

            var mofBody = DscRes.BuildMof(config, cls);
            _log.LogDebug("Generated MOF:\n{0}", mofBody);
            var mofBodyBytes = DscRes.ToUint8(mofBody);
            var methodParams = new CimMethodParametersCollection
            {
                CimMethodParameter.Create("ModuleName", config.ModuleName, CimFlags.None),
                CimMethodParameter.Create("ResourceType", config.TypeName, CimFlags.None),
                CimMethodParameter.Create("resourceProperty", mofBodyBytes, CimFlags.None),
            };

            CimMethodResult methodResult;

            lock (_dscSync) {
                methodResult = ses.InvokeMethod(cim, "ResourceSet", methodParams);
            }
            rebootRequired = (bool)methodResult.OutParameters["RebootRequired"].Value;

            lock (_dscSync) {
                methodResult = ses.InvokeMethod(cim, "ResourceGet", methodParams);
            }
            var dscResConfigs = (CimInstance)methodResult.OutParameters["configurations"].Value;
            
            var state = config;
            state.Results = new Dictionary<string, string>();
            foreach (var p in dscResConfigs.CimInstanceProperties)
            {
                state.Results[p.Name] = p.Value?.ToString() ?? string.Empty;
            }

            lock (_dscSync) {
                methodResult = ses.InvokeMethod(cim, "ResourceTest", methodParams);
            }
            state.InDesiredState = (bool)methodResult.OutParameters["InDesiredState"].Value;

            return state;
        }

    }

    internal static class DscRes
    {
        public const string DefaultCimHostname = "localhost";
        public const string DscCimNamespace  = "root/Microsoft/Windows/DesiredStateConfiguration";
        public const string LcmCimClassName  = "MSFT_DSCLocalConfigurationManager";

        public static string BuildMof(IDscRes res, CimClass cls)
        {
            StringBuilder buff = new StringBuilder();
            
            foreach (var p in res.Properties)
            {
                var cp = cls.CimClassProperties.FirstOrDefault(cp =>
                    cp.Name.Equals(p.Key, StringComparison.OrdinalIgnoreCase));


                var line = $"{p.Key} = {ConvertToQuoted(p.Value, cp.CimType)};";
                buff.AppendLine(line.Replace("\\", "\\\\"));
            }

            var mof = $@"
instance of {res.TypeName}
{{
    ResourceID        = ""[TFDscRes]TFDscRes"";
    ModuleName        = ""{res.ModuleName}"";
    ModuleVersion     = ""{res.ModuleVersion}"";
    ConfigurationName = ""TerrascapeDSC"";
    {buff.ToString()}
}};
instance of OMI_ConfigurationDocument
{{
    Version                  = ""2.0.0"";
    MinimumCompatibleVersion = ""1.0.0"";
}};
";
            return mof;
        }

        public static byte[] ToUint8(string mofBody)
        {
            var mofBodyBytes = Encoding.UTF8.GetBytes(mofBody);
            var total = mofBodyBytes.Length + 4;
            var totalInBytes = BitConverter.GetBytes(total);
            var mofUint8Data = new byte[totalInBytes.Length + mofBodyBytes.Length];

            var copyIndex = 0;
            var copyCount = totalInBytes.Length;
            Array.Copy(totalInBytes, 0, mofUint8Data, copyIndex, copyCount);
            copyIndex += copyCount;
            copyCount = mofBodyBytes.Length;
            Array.Copy(mofBodyBytes, 0, mofUint8Data, copyIndex, copyCount);

            return mofUint8Data;
        }

        public static bool CanConvertTo(string value, CimType type)
        {
            var canConvert = type switch
            {
                CimType.String => true,
                CimType.StringArray => CanConvertToArray<string>(value),
                
                CimType.Boolean => bool.TryParse(value, out _),
                CimType.BooleanArray => CanConvertToArray<bool>(value),

                CimType.Char16 => char.TryParse(value, out _),
                CimType.Char16Array => CanConvertToArray<char>(value),

                CimType.DateTime => DateTime.TryParse(value, out _),
                CimType.DateTimeArray => CanConvertToArray<DateTime>(value),

                CimType.Real32 => float.TryParse(value, out _),
                CimType.Real32Array => CanConvertToArray<float>(value),
                CimType.Real64 => double.TryParse(value, out _),
                CimType.Real64Array => CanConvertToArray<double>(value),

                CimType.SInt64 => long.TryParse(value, out _),
                CimType.SInt64Array => CanConvertToArray<long>(value),
                CimType.SInt32 => int.TryParse(value, out _),
                CimType.SInt32Array => CanConvertToArray<int>(value),
                CimType.SInt16 => short.TryParse(value, out _),
                CimType.SInt16Array => CanConvertToArray<short>(value),
                CimType.SInt8 => sbyte.TryParse(value, out _),
                CimType.SInt8Array => CanConvertToArray<sbyte>(value),

                CimType.UInt64 => ulong.TryParse(value, out _),
                CimType.UInt64Array => CanConvertToArray<ulong>(value),
                CimType.UInt32 => uint.TryParse(value, out _),
                CimType.UInt32Array => CanConvertToArray<uint>(value),
                CimType.UInt16 => ushort.TryParse(value, out _),
                CimType.UInt16Array => CanConvertToArray<ushort>(value),
                CimType.UInt8 => byte.TryParse(value, out _),
                CimType.UInt8Array => CanConvertToArray<byte>(value),

                _ => false,
            };

            return canConvert;
        }

        public static bool CanConvertToArray<T>(string value) => TryConvertToArray<T>(value, out _);

        public static T[] ConvertToArray<T>(string value) =>
            TryConvertToArray<T>(value, out var array)
                ? array
                : throw new Exception("failed to convert to array");

        public static bool TryConvertToArray<T>(string value, out T[] array)
        {
            value = value?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                array = null;
                return true;
            }

            // We only support JSON array notation for now            
            if (value.StartsWith('[') && value.EndsWith(']'))
            {
                try
                {
                    array = JsonSerializer.Deserialize<T[]>(value);
                    return true;
                }
                catch (Exception)
                { }
            }
            array = null;
            return false;
        }

        public static object ConvertToQuoted(string value, CimType type)
        {
            return type switch
            {
                // Basic Types
                CimType.String => value == null ? "NULL" : $"\"{value.Replace("\"", "\\\"")}\"",
                
                CimType.Boolean => bool.Parse(value),
                
                CimType.Char16 => char.Parse(value),
                
                CimType.DateTime => value == null ? "NULL" : $"\"{DateTime.Parse(value)}\"",
                
                CimType.Real32 => float.Parse(value),
                CimType.Real64 => double.Parse(value),
                
                CimType.SInt64 => long.Parse(value),
                CimType.SInt32 => int.Parse(value),
                CimType.SInt16 => short.Parse(value),
                CimType.SInt8 => sbyte.Parse(value),
                
                CimType.UInt64 => ulong.Parse(value),
                CimType.UInt32 => uint.Parse(value),
                CimType.UInt16 => ushort.Parse(value),
                CimType.UInt8 => byte.Parse(value),

                // Arrays of Basic Types
                CimType.StringArray => value == null ? "NULL"
                    : $@"{{{string.Join(",", ConvertToArray<string>(value)
                        ?.Select(v => v == null ? "NULL"
                            : $"\"{v.Replace("\"", "\\\"")}\""))}}}",
                
                CimType.BooleanArray => value == null ? "NULL"
                    : $"{{{string.Join(",", ConvertToArray<bool>(value))}}}",
                
                CimType.Char16Array => value == null ? "NULL"
                    : $"{{{string.Join(",", ConvertToArray<char>(value))}}}",
                
                CimType.DateTimeArray => value == null ? "NULL"
                    : $@"{{{string.Join(",", ConvertToArray<DateTime>(value)
                        ?.Select(v => v == null ? "NULL" : $"\"{v}\""))}}}",
                
                CimType.Real32Array => value == null ? "NULL"
                    : $"{{{string.Join(",", ConvertToArray<float>(value))}}}",

                CimType.Real64Array => value == null ? "NULL"
                    : $"{{{string.Join(",", ConvertToArray<double>(value))}}}",

                CimType.SInt64Array => value == null ? "NULL"
                    : $"{{{string.Join(",", ConvertToArray<long>(value))}}}",

                CimType.SInt32Array => value == null ? "NULL"
                    : $"{{{string.Join(",", ConvertToArray<int>(value))}}}",

                CimType.SInt16Array => value == null ? "NULL"
                    : $"{{{string.Join(",", ConvertToArray<short>(value))}}}",

                CimType.SInt8Array => value == null ? "NULL"
                    : $"{{{string.Join(",", ConvertToArray<sbyte>(value))}}}",

                CimType.UInt64Array => value == null ? "NULL"
                    : $"{{{string.Join(",", ConvertToArray<ulong>(value))}}}",

                CimType.UInt32Array => value == null ? "NULL"
                    : $"{{{string.Join(",", ConvertToArray<uint>(value))}}}",

                CimType.UInt16Array => value == null ? "NULL"
                    : $"{{{string.Join(",", ConvertToArray<ushort>(value))}}}",

                CimType.UInt8Array => value == null ? "NULL"
                    : $"{{{string.Join(",", ConvertToArray<byte>(value))}}}",


                _ => false,
            };
        }
    }
}
