using System;
using System.Collections.Generic;

namespace MsgPackSharp.Converters
{
    public class BasicConverter : IConverter
    {
        public static readonly BasicConverter Instance = new BasicConverter();

        public static readonly IEnumerable<Type> BuiltInTypes = new[]
        {
            // Boolean
            typeof(bool),
            typeof(bool?),

            // Integer
            typeof(byte),
            typeof(byte?),
            typeof(sbyte),
            typeof(sbyte?),
            typeof(short),
            typeof(short?),
            typeof(ushort),
            typeof(ushort?),
            typeof(int),
            typeof(int?),
            typeof(uint),
            typeof(uint?),
            typeof(long),
            typeof(long?),
            typeof(ulong),
            typeof(ulong?),
            typeof(char),
            typeof(char?),

            // Float
            typeof(float),
            typeof(float?),
            typeof(double),
            typeof(double?),
            typeof(decimal),
            typeof(decimal?),

            // Binary
            typeof(byte[]),

            // String
            typeof(string),
        };

        public bool CanDecode(Type type)
        {
            foreach (var t in BuiltInTypes)
                if (t == type)
                    return true;
            
            return false;
        }

        public bool CanEncode(Type type)
        {
            return CanDecode(type);
        }

        public MPObject Encode(IConverterContext ctx, Type type, object obj)
        {
            // Take care of Nullable<> values with null value now
            if (obj == null)
                return MPObject.Nil;

            // Boolean
            if (type == typeof(bool))
                return ((bool)obj) ? MPObject.True : MPObject.False;
            if (type == typeof(bool?))
                return  (((bool?)obj).Value) ? MPObject.True : MPObject.False;

            // Integer
            if (type == typeof(byte))
                return new MPObject(MPType.Integer, (byte)obj);
            if (type == typeof(byte?))
                return new MPObject(MPType.Integer, ((byte?)obj).Value);

            if (type == typeof(sbyte))
                return new MPObject(MPType.Integer, (sbyte)obj);
            if (type == typeof(sbyte?))
                return new MPObject(MPType.Integer, ((sbyte?)obj).Value);

            if (type == typeof(short))
                return new MPObject(MPType.Integer, (short)obj);
            if (type == typeof(short?))
                return new MPObject(MPType.Integer, ((short?)obj).Value);

            if (type == typeof(ushort))
                return new MPObject(MPType.Integer, (ushort)obj);
            if (type == typeof(ushort?))
                return new MPObject(MPType.Integer, ((ushort?)obj).Value);

            if (type == typeof(int))
                return new MPObject(MPType.Integer, (int)obj);
            if (type == typeof(int?))
                return new MPObject(MPType.Integer, ((int?)obj).Value);

            if (type == typeof(uint))
                return new MPObject(MPType.Integer, (uint)obj);
            if (type == typeof(uint?))
                return new MPObject(MPType.Integer, ((uint?)obj).Value);

            if (type == typeof(long))
                return new MPObject(MPType.Integer, (long)obj);
            if (type == typeof(long?))
                return new MPObject(MPType.Integer, ((long?)obj).Value);

            if (type == typeof(ulong))
                return new MPObject(MPType.Integer, (ulong)obj);
            if (type == typeof(ulong?))
                return new MPObject(MPType.Integer, ((ulong?)obj).Value);

            if (type == typeof(char))
                return new MPObject(MPType.Integer, (char)obj);
            if (type == typeof(char?))
                return new MPObject(MPType.Integer, ((char?)obj).Value);

            // Float
            if (type == typeof(float))
                return new MPObject(MPType.Float, (float)obj);
            if (type == typeof(float?))
                return new MPObject(MPType.Float, ((float?)obj).Value);

            if (type == typeof(double))
                return new MPObject(MPType.Float, (double)obj);
            if (type == typeof(double?))
                return new MPObject(MPType.Float, ((double?)obj).Value);

            if (type == typeof(decimal))
                return new MPObject(MPType.Float, (decimal)obj);
            if (type == typeof(decimal?))
                return new MPObject(MPType.Float, ((decimal?)obj).Value);

            // Binary
            if (type == typeof(byte[]))
                return new MPObject(MPType.Binary, (byte[])obj);

            // String
            if (type == typeof(string))
                return new MPObject(MPType.String, (string)obj);

            throw new MPConversionException(type,
                message: $"cannot encode from non-basic target type [{type.FullName}]");
        }

        public object Decode(IConverterContext ctx, Type type, MPObject mpo)
        {
            // Boolean
            if (type == typeof(bool))
                return Decode<bool>(ctx, mpo, MPType.Boolean);
            if (type == typeof(bool?))
                return Decode<bool?>(ctx, mpo, MPType.Boolean);

            // Integer
            if (type == typeof(byte))
                return Decode<byte>(ctx, mpo, MPType.Integer);
            if (type == typeof(byte?))
                return Decode<byte?>(ctx, mpo, MPType.Integer);

            if (type == typeof(sbyte))
                return Decode<sbyte>(ctx, mpo, MPType.Integer);
            if (type == typeof(sbyte?))
                return Decode<sbyte?>(ctx, mpo, MPType.Integer);

            if (type == typeof(short))
                return Decode<short>(ctx, mpo, MPType.Integer);
            if (type == typeof(short?))
                return Decode<short?>(ctx, mpo, MPType.Integer);

            if (type == typeof(ushort))
                return Decode<ushort>(ctx, mpo, MPType.Integer);
            if (type == typeof(ushort?))
                return Decode<ushort?>(ctx, mpo, MPType.Integer);

            if (type == typeof(int))
                return Decode<int>(ctx, mpo, MPType.Integer);
            if (type == typeof(int?))
                return Decode<int?>(ctx, mpo, MPType.Integer);

            if (type == typeof(uint))
                return Decode<uint>(ctx, mpo, MPType.Integer);
            if (type == typeof(uint?))
                return Decode<uint?>(ctx, mpo, MPType.Integer);

            if (type == typeof(long))
                return Decode<long>(ctx, mpo, MPType.Integer);
            if (type == typeof(long?))
                return Decode<long?>(ctx, mpo, MPType.Integer);

            if (type == typeof(ulong))
                return Decode<ulong>(ctx, mpo, MPType.Integer);
            if (type == typeof(ulong?))
                return Decode<ulong?>(ctx, mpo, MPType.Integer);

            if (type == typeof(char))
                return Decode<char>(ctx, mpo, MPType.Integer);
            if (type == typeof(char?))
                return Decode<char?>(ctx, mpo, MPType.Integer);

            // Float
            if (type == typeof(float))
                return Decode<float>(ctx, mpo, MPType.Float);
            if (type == typeof(float?))
                return Decode<float?>(ctx, mpo, MPType.Float);

            if (type == typeof(double))
                return Decode<double>(ctx, mpo, MPType.Float);
            if (type == typeof(double?))
                return Decode<double?>(ctx, mpo, MPType.Float);

            if (type == typeof(decimal))
                return Decode<decimal>(ctx, mpo, MPType.Float);
            if (type == typeof(decimal?))
                return Decode<decimal?>(ctx, mpo, MPType.Float);

            // Binary
            if (type == typeof(byte[]))
                return Decode<byte[]>(ctx, mpo, MPType.Binary);

            // String
            if (type == typeof(string))
                return Decode<string>(ctx, mpo, MPType.String);

            throw new MPConversionException(type, mpo,
                message: $"cannot decode into non-basic target type [{type.FullName}]");
        }

        protected virtual T Decode<T>(IConverterContext ctx, MPObject mpo, MPType mpt)
        {
            var t = typeof(T);
            if (mpo.Type == MPType.Nil)
            {
                if (!t.IsValueType)
                    return default(T);
                if (t.IsGenericType
                    && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return (T)(object)null;
            }

            if (mpo.Type == mpt)
                return (T)mpo.Value;
            
            throw new MPConversionException(typeof(T), mpo,
                message: $"cannot decode MP type [{mpo.Type}][{mpo.Value}] as intended MP type [{mpt}]");
        }

        // public static MPConverter Register(MPConverter conv)
        // {
        //     // Boolean
        //     conv.Register(typeof(bool), Instance);
        //     conv.Register(typeof(bool?), Instance);

        //     // Integer
        //     conv.Register(typeof(byte), Instance);
        //     conv.Register(typeof(byte?), Instance);
        //     conv.Register(typeof(sbyte), Instance);
        //     conv.Register(typeof(sbyte?), Instance);
        //     conv.Register(typeof(short), Instance);
        //     conv.Register(typeof(short?), Instance);
        //     conv.Register(typeof(ushort), Instance);
        //     conv.Register(typeof(ushort?), Instance);
        //     conv.Register(typeof(int), Instance);
        //     conv.Register(typeof(int?), Instance);
        //     conv.Register(typeof(uint), Instance);
        //     conv.Register(typeof(uint?), Instance);
        //     conv.Register(typeof(long), Instance);
        //     conv.Register(typeof(long?), Instance);
        //     conv.Register(typeof(ulong), Instance);
        //     conv.Register(typeof(ulong?), Instance);
        //     conv.Register(typeof(char), Instance);
        //     conv.Register(typeof(char?), Instance);

        //     // Float
        //     conv.Register(typeof(float), Instance);
        //     conv.Register(typeof(float?), Instance);
        //     conv.Register(typeof(double), Instance);
        //     conv.Register(typeof(double?), Instance);
        //     conv.Register(typeof(decimal), Instance);
        //     conv.Register(typeof(decimal?), Instance);

        //     // Binary
        //     conv.Register(typeof(byte[]), Instance);

        //     // String
        //     conv.Register(typeof(string), Instance);

        //     return conv;
        // }

    }
}