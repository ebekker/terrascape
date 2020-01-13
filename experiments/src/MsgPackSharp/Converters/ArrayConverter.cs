using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MsgPackSharp.Converters
{
    public class ArrayConverter : IConverter
    {
        public static readonly ArrayConverter Instance = new ArrayConverter();

        public bool CanDecode(Type type)
        {
            return type.IsArray
                || (type == typeof(IList))
                || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
                || (type.GetConstructor(Type.EmptyTypes) != null
                    && (typeof(IList).IsAssignableFrom(type)
                        || (Util.GetSubclassOfGenericTypeDefinition(typeof(IList<>), type) != null)));
        }

        public bool CanEncode(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public object Decode(IConverterContext ctx, Type type, MPObject mpo)
        {
            if (mpo.Type == MPType.Nil)
                if (!type.IsValueType)
                    return null;
                else
                    throw new MPConversionException(type, mpo,
                        message: $"cannot return null value for target type [{type.FullName}]");

            if (mpo.Type == MPType.Array)
            {
                var mpoArray = (IList<MPObject>)mpo.Value;

                if (type.IsArray)
                {
                    var itemType = type.GetElementType();
                    var arr = Array.CreateInstance(itemType, mpoArray.Count);
                    var ndx = 0;
                    foreach (var mpoItem in mpoArray)
                    {
                        var item = ctx.Decode(itemType, mpoItem);
                        arr.SetValue(item, ndx++);
                    }
                    return arr;
                }

                var gt = Util.GetSubclassOfGenericTypeDefinition(typeof(IList<>), type);
                if (gt != null)
                {
                    var instType = type;
                    var itemType = gt.GenericTypeArguments[0];
                    // If the target type is interface of IList<>, then
                    // we construct a concrete instance type of List<>
                    if (instType.GetGenericTypeDefinition() == typeof(IList<>))
                        instType = typeof(List<>).MakeGenericType(itemType);
                    var inst = Activator.CreateInstance(instType);
                    var meth = instType.GetMethod(nameof(IList<object>.Add), new[] { itemType });
                    foreach (var mpoItem in mpoArray)
                    {
                        var item = ctx.Decode(itemType, mpoItem);
                        meth.Invoke(inst, new[] { item });
                    }
                    return inst;
                }

                if (typeof(IList).IsAssignableFrom(type))
                {
                    var arr = type == typeof(IList)
                        ? new ArrayList(mpoArray.Count)
                        : (IList)Activator.CreateInstance(type);
                    foreach (var mpoItem in mpoArray)
                    {
                        var item = ctx.Decode(typeof(object), mpoItem);
                        arr.Add(item);
                    }
                    return arr;
                }
            }

            throw new MPConversionException(type, mpo,
                message: $"could not decode [{mpo.Type}] as array object");
        }

        public MPObject Encode(IConverterContext ctx, Type type, object obj)
        {
            if (obj == null)
                return MPObject.Nil;
            
            var itemType = typeof(object);
            var gt = Util.GetSubclassOfGenericTypeDefinition(typeof(IList<>), type);
            if (type.IsArray)
            {
                itemType = type.GetElementType();
            }
            else if (gt != null)
            {
                itemType = gt.GenericTypeArguments[0];
            }

            var arr = new List<MPObject>();
            var enm = (IEnumerable)obj;
            foreach (var item in enm)
                arr.Add(ctx.Encode(itemType, item));
            
            return new MPObject(MPType.Array, arr);
        }
    }
}