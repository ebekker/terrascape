using System;
using System.Collections;
using System.Collections.Generic;

namespace MsgPackSharp.Converters
{
    public class MapConverter : IConverter
    {
        public static readonly MapConverter Instance = new MapConverter();

        public bool CanDecode(Type type)
        {
            return type == typeof(IDictionary)
                || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                || (type.GetConstructor(Type.EmptyTypes) != null
                    && (typeof(IDictionary).IsAssignableFrom(type)
                        || Util.GetSubclassOfGenericTypeDefinition(typeof(IDictionary<,>), type) != null));
        }

        public bool CanEncode(Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type)
                || Util.GetSubclassOfGenericTypeDefinition(typeof(IDictionary<,>), type) != null;
        }

        public object Decode(IConverterContext ctx, Type type, MPObject mpo)
        {
            if (mpo.Type == MPType.Nil)
                if (!type.IsValueType)
                    return null;
                else
                    throw new MPConversionException(type, mpo,
                        message: $"cannot return null value for target type [{type.FullName}]");
            
            if (mpo.Type == MPType.Map)
            {
                var mpoMap = (Dictionary<MPObject, MPObject>)mpo.Value;

                var gt = Util.GetSubclassOfGenericTypeDefinition(typeof(IDictionary<,>), type);
                if (gt != null)
                {
                    var instType = type;
                    var keyType = gt.GenericTypeArguments[0];
                    var valType = gt.GenericTypeArguments[1];
                    // If the target type is interface of IDictionary<,>, then
                    // we construct a concrete instance type of Dictionary<,>
                    if (instType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                        instType = typeof(IDictionary<,>).MakeGenericType(keyType, valType);
                    var inst = Activator.CreateInstance(instType);
                    var meth = instType.GetMethod(nameof(IDictionary<object, object>.Add), new[] { keyType, valType });
                    foreach (var kv in mpoMap)
                    {
                        var k = ctx.Decode(keyType, kv.Key);
                        var v = ctx.Decode(valType, kv.Value);
                        meth.Invoke(inst, new[] { k, v });
                    }
                    return inst;
                }

                if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    var map = type == typeof(IDictionary)
                        ? new Hashtable(mpoMap.Count)
                        : (IDictionary)Activator.CreateInstance(type);
                    foreach (var kv in mpoMap)
                    {
                        var k = ctx.Decode(typeof(object), kv.Key);
                        var v = ctx.Decode(typeof(object), kv.Value);
                        map.Add(k, v);
                    }
                    return map;
                }
            }

            throw new MPConversionException(type, mpo,
                message: $"could not decode [{mpo.Type}] as map object");
        }

        public MPObject Encode(IConverterContext ctx, Type type, object obj)
        {
            if (obj == null)
                return MPObject.Nil;
            
            var keyType = typeof(object);
            var valType = typeof(object);
            IEnumerable keys;
            Func<object, object> getter;

            var gt = Util.GetSubclassOfGenericTypeDefinition(typeof(IDictionary<,>), type);
            if (gt != null)
            {
                keyType = gt.GenericTypeArguments[0];
                valType = gt.GenericTypeArguments[1];
                keys = (IEnumerable)gt.GetProperty("Keys").GetValue(obj);
                var itemProp = gt.GetProperty("Item");
                var index = new object[] { null };
                getter = k => {
                    index[0] = k;
                    return itemProp.GetValue(obj, index);
                };
            }
            else
            {
                var d = (IDictionary)obj;
                keys = d.Keys;
                getter = k => d[k];
            }

            var map = new Dictionary<MPObject, MPObject>();
            foreach (var k in keys)
            {
                var v = getter(k);
                var mpoKey = ctx.Encode(keyType, k);
                var mpoVal = ctx.Encode(valType, v);
                map.Add(mpoKey, mpoVal);
            }
            
            return new MPObject(MPType.Map, map);
        }
    }
}