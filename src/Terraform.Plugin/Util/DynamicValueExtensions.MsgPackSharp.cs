using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using MsgPackSharp;
using Terraform.Plugin.Attributes;
using Tfplugin5;

namespace Terraform.Plugin.Util
{
    public static partial class DynamicValueExtensions
    {
        private static ILogger _log =
            TFPluginServer.LoggerFactory.CreateLogger(typeof(DynamicValueExtensions));

        private static readonly MPConverterContext InnerConverterContext =
            (MPConverterContext)MPConverterContext.CreateDefault(new[]
            {
                TFSchemaObjectConverter.Instance
            }, true);

        private static readonly TFConverterContext WithNullsConverterContext =
            new TFConverterContext(InnerConverterContext.Converters, false);
        private static readonly TFConverterContext WithUnknownConverterContext =
            new TFConverterContext(InnerConverterContext.Converters, true);

        public static DynamicValue MarshalViaMPSharp(Type t, object value, bool withUnknowns)
        {
            using (var ms = new MemoryStream())
            {
                var mpo = withUnknowns
                    ? WithUnknownConverterContext.Encode(t, value)
                    : WithNullsConverterContext.Encode(t, value);

                _log.LogTrace("Marshalled to MPObject:");
                Visit(mpo, (l, s) => _log.LogTrace("MPVisitor: {0}", s));

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

                _log.LogTrace("Unmarshalled to MPOjbject:");
                Visit(mpo.Value, (l, s) => _log.LogTrace("MPVisitor: {0}", s));

                if (MPReader.Parse(ms) != null)
                    _log.LogWarning("DynamicValue message pack stream has more to read -- UNEXPECTED!!");

                
                return WithNullsConverterContext.Decode(t, mpo.Value);
            }
        }

        public class MPVisitor
        {
            public int _level = 0;
            public string _indent = string.Empty;

            public static MPVisitor operator ++(MPVisitor v)
            {
                ++v._level;
                v._indent = string.Join("", Enumerable.Repeat("  ", v._level));
                return v;
            }

            public static MPVisitor operator --(MPVisitor v)
            {
                --v._level;
                v._indent = string.Join("", Enumerable.Repeat("  ", v._level));
                return v;
            }
        }

        public static void Visit(MPObject mpo, Action<int, string> action, MPVisitor v = null)
        {
            if (v == null)
                v = new MPVisitor();

            switch (mpo.Type)
            {
                case MPType.Nil:
                    action(v._level, $"{v._indent}(nil)");
                    break;
                case MPType.Ext:
                    var ext = (MPExt)mpo.Value;
                    action(v._level, $"{v._indent}Ext(type={ext.Type};data={BitConverter.ToString(ext.Data.ToArray())})");
                    break;
                case MPType.Binary:
                    var bin = (byte[])mpo.Value;
                    action(v._level, $"{v._indent}Bin({Convert.ToBase64String(bin)})");
                    break;
                case MPType.Array:
                    var arr = (System.Collections.IList)mpo.Value;
                    action(v._level, $"{v._indent}Array({arr?.Count}):");
                    ++v;
                    foreach (var val in arr)
                        Visit((MPObject)val, action, v);
                    --v;
                    break;
                case MPType.Map:
                    var map = (System.Collections.IDictionary)mpo.Value;
                    action(v._level, $"{v._indent}Map({map?.Count}):");
                    ++v;
                    foreach (var key in map.Keys)
                    {
                        Visit((MPObject)key, action, v);
                        ++v;
                            Visit((MPObject)map[key], action, v);
                            --v;
                    }
                    --v;
                    break;
                default:
                    action(v._level, $"{v._indent}{mpo.Type}({mpo.Value}):");
                    break;
            }
        }

        /// <summary>
        /// Implements some custom logic for handling special
        /// EXT formats when reading the base data types.
        /// </summary>
        public class TFConverterContext : MPConverterContext
        {
            public TFConverterContext(IEnumerable<IConverter> converters, bool withUnknowns)
            {
                WithUnknowns = withUnknowns;

                foreach (var c in converters)
                    base.Converters.Add(c);
            }

            public bool WithUnknowns { get; }

            public override MPObject Encode(Type type, object obj)
            {
                var mpo = base.Encode(type, obj);

                if (WithUnknowns && mpo.Type == MPType.Nil)
                    mpo = TFSchemaObjectConverter.UnknownExtensionObject;

                return mpo;
            }

            public override object Decode(Type type, MPObject mpo)
            {
                if (mpo.Type == MPType.Ext)
                {
                    _log.LogDebug("Found potential computed value");

                    var ext = (MPExt)mpo.Value;
                    if (TFSchemaObjectConverter.UnknownExtension.Equals(ext)
                    || TFSchemaObjectConverter.UnknownExtensionAlt.Equals(ext))
                    {
                        _log.LogDebug("Computed value being decoded as NIL");
                        return Decode(type, MPObject.Nil);
                    }
                    else
                    {
                        _log.LogWarning("Found unexpected EXT value: [{0}] not matching either [{1}] or [{2}]",
                            ext, TFSchemaObjectConverter.UnknownExtension,
                            TFSchemaObjectConverter.UnknownExtensionAlt);
                    }
                }

                return  base.Decode(type, mpo);
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

            public static readonly MPObject UnknownExtensionObject =
                new MPObject(MPType.Ext, UnknownExtension);
            public static readonly MPObject UnknownExtensionAltObject =
                new MPObject(MPType.Ext, UnknownExtensionAlt);

            public bool CanEncode(Type type)
            {
                var result = CanEncodeDecode(type);
                _log.LogDebug("Can encode [{type}] => [{result}]", type, result);
                return result;
            }

            public bool CanDecode(Type type)
            {
                var result = CanEncodeDecode(type);
                _log.LogDebug("Can decode [{type}] => [{result}]", type, result);
                return result;
            }

            private bool CanEncodeDecode(Type type) =>
                !type.IsValueType && type.GetConstructor(Type.EmptyTypes) != null;

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
                    MPObject propValue;
                    
                    if (p.attr.Computed)
                    {
                        if (value == null)
                        {
                            propValue = new MPObject(MPType.Ext, UnknownExtension);
                        }
                        else
                        {
                            propValue = ctx.Encode(propType, value);
                        }
                    }
                    else
                    {
                        propValue = WithNullsConverterContext.Encode(propType, value);
                    }
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
                        if (kv.Key.Type != MPType.String)
                            throw new NotSupportedException(
                                $"got a map key that was not a string: [{kv.Key}]");

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

            // DO WE STILL NEED THESE???

            // public MPObject Encode(IConverterContext ctx, Schema schema, object obj)
            // {
            //     if (obj == null)
            //         return MPObject.Nil;
                
            //     var map = new Dictionary<MPObject, MPObject>();
            //     var type = obj.GetType();
            //     foreach (var a in schema.Block.Attributes)
            //     {
            //         var prop = type.GetProperty(a.Name,
            //             BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            //         var value = prop.GetValue(obj);
                    
            //         var propName = ctx.Encode(typeof(string), a.Name);
            //         var propType = prop.PropertyType;
            //         var propValue = (value == null && a.Computed)
            //             ? new MPObject(MPType.Ext, UnknownExtension)
            //             : ctx.Encode(propType, value);
            //         map.Add(propName, propValue);
            //     }
            //     return new MPObject(MPType.Map, map);
            // }

            // public T Decode<T>(IConverterContext ctx, Schema schema, MPObject mpo, T inst = null)
            //     where T : class
            // {
            //     if (inst == null)
            //         inst = Activator.CreateInstance<T>();
                
            //     return (T)Decode(ctx, schema, mpo, (object)inst);
            // }

            // public object Decode(IConverterContext ctx, Schema schema, MPObject mpo, object inst)
            // {
            //     _log.LogDebug("Decoding Schema Object");
            //     if (mpo.Type == MPType.Nil)
            //         return null;

            //     var type = inst.GetType();

            //     if (mpo.Type == MPType.Map)
            //     {
            //         var map = (IDictionary<MPObject, MPObject>)mpo.Value;
            //         var attrMap = schema.Block.Attributes.ToDictionary(a => a.Name, a => a);

            //         _log.LogDebug($"decoding map of [{map.Count}] values to [{schema.Block.Attributes.Count}] properties on [{type.FullName}]");


            //         foreach (var kv in map)
            //         {
            //             var name = (string)ctx.Decode(typeof(string), kv.Key);
            //             if (!attrMap.TryGetValue(name, out var pa))
            //                 throw new MPConversionException(type, mpo,
            //                     message: $"could not resolve map key to type property [{name}]");

            //             var prop = type.GetProperty(name,
            //                 BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            //             if (kv.Value.Type == MPType.Ext)
            //             {
            //                 var ext = (MPExt)kv.Value.Value;
            //                 _log.LogDebug($"encountered EXT value [{ext.Type}][{ext.Data.Length}]"
            //                     + $" for key [{name}] computed=[{pa.Computed}]");
            //                 if (pa.Computed && (ext.Type == UnknownValExtTypeCode
            //                     || ext.Type == UnknownValExtTypeCodeAlt))
            //                 {
            //                     _log.LogDebug("    signal for computed property, setting no value");
            //                     prop.SetValue(inst, null);
            //                     continue;
            //                 }
            //                 _log.LogWarning("unexpected EXT encountered");
            //             }

            //             var value = ctx.Decode(prop.PropertyType, kv.Value);
            //             prop.SetValue(inst, value);
            //         }

            //         return inst;
            //     }

            //     throw new MPConversionException(type, mpo,
            //         message: $"could not decode [{mpo.Type}] as object");
            // }
        }
    }
}