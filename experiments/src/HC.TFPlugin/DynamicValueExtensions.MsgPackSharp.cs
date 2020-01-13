using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Google.Protobuf;
using HC.TFPlugin.Attributes;
using Microsoft.Extensions.Logging;
using MsgPackSharp;
using Tfplugin5;

namespace HC.TFPlugin
{
    public static partial class DynamicValueExtensions
    {
        private static readonly IConverterContext ConverterContext =
            MPConverterContext.CreateDefault(new[] { TFSchemaObjectConverter.Instance }, true);

        public static DynamicValue MarshalViaMPSharp(Type t, object value)
        {
            using (var ms = new MemoryStream())
            {
                var mpo = ConverterContext.Encode(t, value);
                var mpw = new MPWriter(ms);
                _log.LogDebug($"emitting [{t.FullName}] as MsgPack");
                mpw.Emit(mpo);

                return new DynamicValue
                {
                    Msgpack = ByteString.CopyFrom(ms.ToArray()),
                };
            }            
        }

        public static object UnmarshalViaMPSharp(Type t, DynamicValue dv)
        {
            using (var ms = new MemoryStream(dv.Msgpack.ToArray()))
            {
                _log.LogDebug($"parsing MsgPack as [{t.FullName}]");
                var mpo = MPReader.Parse(ms);
                if (mpo == null)
                    throw new InvalidDataException("premature EOF");
                return ConverterContext.Decode(t, mpo.Value);
            }
        }

        public class TFSchemaObjectConverter : IConverter
        {
            public static readonly TFSchemaObjectConverter Instance = new TFSchemaObjectConverter();

            // This is used to signal an "Unknown Value" which is interpreted by
            // Terraform as "Computed After Apply"
            //  https://github.com/zclconf/go-cty/blob/master/cty/msgpack/unknown.go
            private static readonly byte UnknownValExtTypeCode = 0xd4;
            private static readonly byte[] UnknownValExtArgBytes = new byte[] { 0, 0 };

            // It seems that when unmarshaling we're running into an alternate ExtTypeCode
            // for "Unknown Values" -- not sure why they don't conform to the above docs
            //  https://github.com/zclconf/go-cty/blob/master/cty/msgpack/doc.go
            private static readonly byte UnknownValExtTypeCodeAlt = 0x00;
            private static readonly byte[] UnknownValExtArgBytesAlt = new byte[] { 0 };

            public static readonly MPExt UnknownExtension =
                new MPExt(UnknownValExtTypeCode, UnknownValExtArgBytes);
            public static readonly MPExt UnknownExtensionAlt =
                new MPExt(UnknownValExtTypeCodeAlt, UnknownValExtArgBytesAlt);

            public bool CanEncode(Type type)
            {
                return !type.IsValueType && type.GetConstructor(Type.EmptyTypes) != null;
            }

            public bool CanDecode(Type type)
            {
                return CanEncode(type);
            }

            public MPObject Encode(IConverterContext ctx, Type type, object obj)
            {
                if (obj == null)
                    return MPObject.Nil;
                
                var map = new Dictionary<MPObject, MPObject>();
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => (prop: p, attr: p.GetCustomAttribute<TFAttributeAttribute>()))
                    .Where(pa => pa.attr != null)
                    .ToArray();
                foreach (var p in props)
                {
                    var name = p.attr.Name;
                    var value = p.prop.GetValue(obj);

                    var propName = ctx.Encode(typeof(string), name);
                    var propType = p.prop.PropertyType;
                    var propValue = (value == null && p.attr.Computed)
                        ? new MPObject(MPType.Ext, UnknownExtension)
                        : ctx.Encode(propType, value);
                    map.Add(propName, propValue);
                }
                return new MPObject(MPType.Map, map);
            }

            public object Decode(IConverterContext ctx, Type type, MPObject mpo)
            {
                _log.LogDebug("Decoding Schema Object");
                if (mpo.Type == MPType.Nil)
                    return null;

                if (mpo.Type == MPType.Map)
                {
                    var map = (IDictionary<MPObject, MPObject>)mpo.Value;
                    var inst = Activator.CreateInstance(type);
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(p => (prop: p, attr: p.GetCustomAttribute<TFAttributeAttribute>()))
                        .Where(pa => pa.attr != null)
                        .ToDictionary(pa => pa.attr.Name, pa => pa);

                    _log.LogDebug($"decoding map of [{map.Count}] values to [{props.Count}] properties on [{type.FullName}]");

                    foreach (var kv in map)
                    {
                        var name = (string)ctx.Decode(typeof(string), kv.Key);
                        if (!props.TryGetValue(name, out var pa))
                            throw new MPConversionException(type, mpo,
                                message: $"could not resolve map key to type property [{name}]");

                        if (kv.Value.Type == MPType.Ext)
                        {
                            var ext = (MPExt)kv.Value.Value;
                            _log.LogDebug($"encountered EXT value [{ext.Type}][{ext.Data.Length}]"
                                + $" for key [{name}] computed=[{pa.attr.Computed}]");
                            if (pa.attr.Computed && (ext.Type == UnknownValExtTypeCode
                                || ext.Type == UnknownValExtTypeCodeAlt))
                            {
                                _log.LogDebug("    signal for computed property, setting no value");
                                pa.prop.SetValue(inst, null);
                                continue;
                            }
                            _log.LogWarning("unexpected EXT encountered");
                        }

                        var value = ctx.Decode(pa.prop.PropertyType, kv.Value);
                        pa.prop.SetValue(inst, value);
                    }

                    return inst;
                }

                throw new MPConversionException(type, mpo,
                    message: $"could not decode [{mpo.Type}] as object");
            }
        }
    }
}