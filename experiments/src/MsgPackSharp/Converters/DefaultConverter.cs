using System;
using System.Collections;
using System.Collections.Generic;

namespace MsgPackSharp.Converters
{
    public class DefaultConverter : IConverter
    {
        public static readonly DefaultConverter Instance = new DefaultConverter();
        
        public bool CanDecode(Type type)
        {
            return type == typeof(object);
        }

        public bool CanEncode(Type type)
        {
            return type == typeof(object);
        }

        public object Decode(IConverterContext ctx, Type type, MPObject mpo)
        {
            switch (mpo.Type)
            {
                case MPType.Array:
                    return ctx.Decode(typeof(ArrayList), mpo);
                case MPType.Map:
                    return ctx.Decode(typeof(Hashtable), mpo);
                default:
                    return mpo.Value;
            }
        }

        public MPObject Encode(IConverterContext ctx, Type type, object obj)
        {
            Type narrowType = null;

            if (obj == null)
                return MPObject.Nil;

            if (obj is bool || obj is bool?
                || obj is byte || obj is byte?
                || obj is sbyte || obj is sbyte?
                || obj is short || obj is short?
                || obj is ushort || obj is ushort?
                || obj is int || obj is int?
                || obj is uint || obj is uint?
                || obj is long || obj is long?
                || obj is ulong || obj is ulong?
                || obj is char || obj is char?
                || obj is float || obj is float?
                || obj is double || obj is double?
                || obj is decimal || obj is decimal?
                || obj is byte[] || obj is string
                )
            {
                narrowType = obj.GetType();
            }
            else if (obj is IDictionary
                || Util.GetSubclassOfGenericTypeDefinition(
                    typeof(IDictionary<,>), obj.GetType()) != null)
            {
                narrowType = typeof(Hashtable);
            }
            else if (obj is Array || obj is IList
                || (Util.GetSubclassOfGenericTypeDefinition(
                    typeof(IList<>), obj.GetType()) != null))
            {
                narrowType = typeof(ArrayList);
            }

            if (narrowType != null)
                return ctx.Encode(narrowType, obj);

            throw new MPConversionException(type);
        }

        // public MPObject EncodeOld(MPConverterContext ctx, Type type, object obj)
        // {
        //     MPType? t = null;
        //     object v = obj;

        //     if (obj == null)
        //     {
        //         t = MPType.Nil;
        //     }
        //     else if (obj is bool)
        //     {
        //         t = MPType.Boolean;
        //     }
        //     else if (obj is byte || obj is sbyte
        //         || obj is short || obj is ushort
        //         || obj is int || obj is uint
        //         || obj is long || obj is ulong
        //         || obj is char)
        //     {
        //         t = MPType.Integer;
        //     }
        //     else if (obj is byte? || obj is sbyte?
        //         || obj is short? || obj is ushort?
        //         || obj is int? || obj is uint?
        //         || obj is long? || obj is ulong?
        //         || obj is char?)
        //     {
        //         t = MPType.Integer;
        //         v = (long)obj;
        //     }
        //     else if (obj is float || obj is double || obj is decimal)
        //     {
        //         t = MPType.Float;
        //     }
        //     else if (obj is float? || obj is double? || obj is decimal?)
        //     {
        //         t = MPType.Float;
        //         v = (double)obj;
        //     }
        //     else if (obj is byte[])
        //     {
        //         t = MPType.Binary;
        //     }
        //     else if (obj is string)
        //     {
        //         t = MPType.String;
        //     }
        //     else if (obj is IDictionary
        //         || Util.GetSubclassOfGenericTypeDefinition(
        //             typeof(IDictionary<,>), obj.GetType()) != null)
        //     {
        //         t = MPType.Map;
        //     }
        //     else if (obj is Array || obj is IList
        //         || (Util.GetSubclassOfGenericTypeDefinition(
        //             typeof(IList<>), obj.GetType()) != null))
        //     {
        //         t = MPType.Array;
        //         v = ctx.Encode(typeof(IList), v).Value;
        //     }

        //     if (t != null)
        //         return new MPObject(t.Value, v);
            
        //     throw new MPConversionException(type);
        // }
    }
}