using System;
using System.Collections;
using System.Collections.Generic;

namespace MsgPackSharp.Converters
{
    public class ReadOnlyMapConverter : IConverter
    {
        public static readonly ReadOnlyMapConverter Instance = new ReadOnlyMapConverter();

        public bool CanDecode(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
                || (type.GetConstructor(Type.EmptyTypes) != null
                    && (Util.GetSubclassOfGenericTypeDefinition(typeof(IDictionary<,>), type) != null));
        }

        public bool CanEncode(Type type)
        {
            return Util.GetSubclassOfGenericTypeDefinition(typeof(IReadOnlyDictionary<,>), type) != null;
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
                    // If the target type is interface of IReadOnlyDictionary<,>, then
                    // we construct a concrete instance type of Dictionary<,>
                    if (instType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
                        instType = typeof(IReadOnlyDictionary<,>).MakeGenericType(keyType, valType);
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

            var gt = Util.GetSubclassOfGenericTypeDefinition(typeof(IReadOnlyDictionary<,>), type);

            keyType = gt.GenericTypeArguments[0];
            valType = gt.GenericTypeArguments[1];
            keys = (IEnumerable)gt.GetProperty("Keys").GetValue(obj);
            var itemProp = gt.GetProperty("Item");
            var index = new object[] { null };
            getter = k => {
                index[0] = k;
                return itemProp.GetValue(obj, index);
            };

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