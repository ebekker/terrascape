using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Google.Protobuf;
using HC.TFPlugin.Attributes;
using Microsoft.Extensions.Logging;
using MsgPack;
using MsgPack.Serialization;
using Tfplugin5;

namespace HC.TFPlugin
{
    public static partial class DynamicValueExtensions
    {
        public static DynamicValue MarshalViaMsgPackCli(Type t, object value)
        {
            var ctx = new SerializationContext
            {
                SerializationMethod = SerializationMethod.Map,
            };
            ctx.ResolveSerializer += (s, ev) => {
                _log.LogInformation("Resolving Serialization for: " + ev.TargetType.FullName);
                var serType = typeof(TFMsgPackSerializer<>).MakeGenericType(ev.TargetType);
                var serInst = Activator.CreateInstance(serType, ev.Context);
                ((ISerSetter)serInst).SetSerializer(ev);
            };
            var ser = MessagePackSerializer.Get(t, ctx);
            using (var ms = new MemoryStream())
            {
                ser.Pack(ms, value);
                ms.Flush();

                return new DynamicValue
                {
                    Msgpack = ByteString.CopyFrom(ms.ToArray()),
                };
            }
        }

        public static object UnmarshalViaMsgPackCli(Type t, DynamicValue dv)
        {
            var ctx = new SerializationContext
            {
                SerializationMethod = SerializationMethod.Map,
            };
            ctx.ResolveSerializer += (s, ev) => {
                _log.LogInformation("Resolving Serialization for: " + ev.TargetType.FullName);
                var serType = typeof(TFMsgPackSerializer<>).MakeGenericType(ev.TargetType);
                var serInst = Activator.CreateInstance(serType, ev.Context);
                ((ISerSetter)serInst).SetSerializer(ev);
            };
            var ser = MessagePackSerializer.Get(t, ctx);
            using (var ms = new MemoryStream(dv.Msgpack.ToArray()))
            {
                return ser.Unpack(ms);
            }
        }
    }

    public interface ISerSetter
    {
        void SetSerializer(ResolveSerializerEventArgs ev);
    }

    public class TFMsgPackSerializer<T> : MessagePackSerializer<T>, ISerSetter
    {
        private static ILogger _log = LogUtil.Create<TFMsgPackSerializer<T>>();

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

        public static readonly SerializationContext MapSerializationContext;

        public TFMsgPackSerializer() : base(MapSerializationContext)
        { }

        public TFMsgPackSerializer(SerializationContext ownerContext) : base(ownerContext)
        { }

        public void SetSerializer(ResolveSerializerEventArgs ev) => ev.SetSerializer<T>(this);

        protected override void PackToCore(Packer packer, T objectTree)
        {
            _log.LogDebug("***** PACKING " + typeof(T).FullName);
            PackMap(packer, typeof(T), objectTree);
        }

        protected void PackMap(Packer packer, Type type, object inst)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => (prop: p, attr: p.GetCustomAttribute<TFAttributeAttribute>()))
                .Where(pa => pa.attr != null)
                .ToArray();
            packer.PackMapHeader(props.Length);
            foreach (var p in props)
            {
                var name = p.attr.Name;
                var value = p.prop.GetValue(inst);

                packer.PackString(name);
                if (p.attr.Computed && value == null)
                {
                    _log.LogDebug("Packing UNKNOWN VALUE Extended Type Code");
                    packer.PackExtendedTypeValue(UnknownValExtTypeCode, UnknownValExtArgBytes);
                }
                else if (p.attr.Nested)
                {
                    if (value == null)
                    {
                        _log.LogWarning("Nested property has NULL value, skipping");
                    }
                    else
                    {
                        var propType = p.prop.PropertyType;
                        if (TypeMapper.MapElementTypeFrom(propType) is Type mapElementType)
                            packer.PackObject(value);
                        else if (TypeMapper.ListElementTypeFrom(propType) is Type listElementType)
                            packer.PackObject(value);
                        else
                            PackMap(packer, propType, value);
                    }
                }
                else
                {
                    packer.PackObject(value);
                }
            }
        }

        protected override T UnpackFromCore(Unpacker unpacker)
        {
            // // // _log.LogDebug("***** UNPACKING " + typeof(T).FullName);
            // // // _log.LogDebug("  Collection: " + unpacker.IsCollectionHeader);
            // // // _log.LogDebug("  Array: " + unpacker.IsArrayHeader);
            // // // _log.LogDebug("  Map: " + unpacker.IsMapHeader);
            
            // // // unpacker.ReadMapLength(out var mapLen);
            // // // _log.LogDebug("  mapLen: " + mapLen);

            // // // var mpo = unpacker.ReadItem();
            // // // //unpacker.ReadObject(out var mpo);
            // // // _log.LogDebug("  NextUp: " + mpo?.UnderlyingType.FullName);

            var map = unpacker.Unpack<Dictionary<string, object>>();
            if (map == null)
                throw new InvalidDataException("unable to unpack expected map serialization");
            return (T)UnpackMap(map, typeof(T));

            // unpacker.ReadObject(out var result);
            // if (result.IsList) // (unpacker.IsArrayHeader)
            // {
            //     var list = result.AsList(); // unpacker.Unpack<List<object>>();
            //     if (list == null)
            //         throw new InvalidDataException("unable to unpack expected list serialization");

            //     return (T)(object)list;
            // }
            // else if (result.IsMap) // (unpacker.IsMapHeader)
            // {
            //     var map = result.AsDictionary().ToDictionary(
            //         kv => kv.Key.ToString(),
            //         kv => kv.Value.ToObject()); // unpacker.Unpack<Dictionary<string, object>>();
            //     if (map == null)
            //         throw new InvalidDataException("unable to unpack expected map serialization");

            //     return (T)UnpackMap(map, typeof(T));
            // }
            // // else if (result is MessagePackObject mpo) // (unpacker.IsMapHeader)
            // // {
            // //     mpo.ToObject();
            // //     var map = mpo.AsDictionary().ToDictionary(
            // //         kv => kv.Key.ToString(),
            // //         kv => kv.Value.ToObject()); // unpacker.Unpack<Dictionary<string, object>>();
            // //     if (map == null)
            // //         throw new InvalidDataException("unable to unpack expected map serialization");

            // //     return (T)UnpackMap(map, typeof(T));
            // // }
            // else
            // {
            //     if (result is MessagePackObject mpo && mpo.IsNil)
            //     {
            //         _log.LogDebug("MPO is NIL");

            //         return Activator.CreateInstance<T>();

            //         //return default(T);
            //     }

            //     var val = mpo.ToObject();
            //     _log.LogWarning($"cannot resolve MPO: [{val.GetType().FullName}][{val}]");
            //     throw new InvalidDataException("unexpected serialized data: " + result.GetType().FullName);
            // }
        }

        protected object UnpackMap(IDictionary<string, object> map, Type type)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => (prop: p, attr: p.GetCustomAttribute<TFAttributeAttribute>()))
                .Where(pa => pa.attr != null)
                .ToArray();

            var inst = Activator.CreateInstance(type);
            foreach (var p in props)
            {
                var name = p.attr.Name;
                if (!map.TryGetValue(name, out var value))
                {
                    _log.LogWarning("missing map entry for [{propName}]", name);
                    continue;
                }
                
                if (value == null)
                {
                    _log.LogWarning("null value for [{propName}]", name);
                    continue;
                }

                if (value is MessagePackObject mpo)
                    value = mpo.ToObject();

                if (value is MessagePackExtendedTypeObject mpeto)
                {
                    if (!mpeto.IsValid)
                        _log.LogWarning("Invalid Ext Type for [{propName}]", name);
                    else if (UnknownValExtTypeCode != mpeto.TypeCode
                        && UnknownValExtTypeCodeAlt != mpeto.TypeCode)
                        // _log.LogWarning("Unexpected Ext Type TypeCode for [{propName}]: [{typeCode}][{typeBody}]",
                        //     name, mpeto.TypeCode, BitConverter.ToString(mpeto.GetBody()));
                        throw new Exception("UNKNOWN");
                    else if (!UnknownValExtArgBytes.SequenceEqual(mpeto.GetBody())
                        && !UnknownValExtArgBytesAlt.SequenceEqual(mpeto.GetBody()))
                        // _log.LogWarning("Unexpected Ext Type Body for [{propName}]: [{typeCode}][{typeBody}]",
                        //     name, mpeto.TypeCode, BitConverter.ToString(mpeto.GetBody()));
                        throw new Exception("UNKNOWN");
                    else
                        _log.LogWarning("UnknownValue Ext Type for [{propName}]", name);
                    continue;
                }

                // if (p.attr.Nested)
                // {
                //     var propType = p.prop.PropertyType;
                //     if (TypeMapper.ListElementTypeFrom(propType) is Type listElementType)
                //     {
                //         if (value is MessagePackObject mpoList && mpoList.IsList)
                //         {
                //             IList list = (IList)Activator.CreateInstance(listElementType);
                //             foreach (var item in mpoList.AsList())
                //             {
                //                 list.Add(item.ToObject());
                //             }
                //             value = list;
                //         }
                //         else
                //         {
                //             _log.LogWarning("incompatible deserialized value with nested list");
                //         }
                //     }
                //     else if (TypeMapper.MapElementTypeFrom(propType) is Type mapElementType)
                //     {
                //         if (value is MessagePackObjectDictionary mapValue)
                //         {
                //             value = UnpackMap(mapValue.ToDictionary(
                //                 kv => kv.Key.AsString(),
                //                 kv => kv.Value.ToObject()), p.prop.PropertyType);
                //         }
                //         else
                //         {
                //             _log.LogWarning("incompatible deserialized value with nested map");
                //         }
                //     }
                //     if (value is MessagePackObjectDictionary mapObjectValue)
                //     {
                //         value = UnpackMap(mapObjectValue.ToDictionary(
                //             kv => kv.Key.AsString(),
                //             kv => kv.Value.ToObject()), p.prop.PropertyType);
                //     }
                //     else
                //     {
                //         _log.LogWarning("incompatible value for NESTED [{propName}]: [{propType}] [{valType}]",
                //             name, p.prop.PropertyType.FullName, value.GetType().FullName);
                //         continue;
                //     }
                // }

                if (!p.prop.PropertyType.IsInstanceOfType(value))
                {
                    _log.LogWarning("incompatible value for [{propName}]: [{propType}] [{valType}]",
                        name, p.prop.PropertyType.FullName, value.GetType().FullName);
                    continue;
                }
                
                p.prop.SetValue(inst, value);
            }
            return inst;
        }
    }
}