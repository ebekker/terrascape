using System;

namespace MsgPackSharp.Converters
{
    public class CommonConverter : IConverter
    {
        public static readonly CommonConverter DefaultInstance = new CommonConverter();

        public CommonConverter(
            bool encodeEnumAsNameString = false,
            bool encodeGuidAsString = false)
        {
            EncodeEnumAsNameString = encodeEnumAsNameString;
            EncodeGuidAsString = encodeGuidAsString;
        }

        public bool EncodeEnumAsNameString { get; }

        public bool EncodeGuidAsString { get; }

        // Enum
        // DateTime
        // Byte[]
        // Type
        // Guid

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Supports encoding/decoding of common value types:
        /// * Enum - encoding/decoding as integer or string
        /// * Guid - encoding/decoding as binary (byte array) or string
        /// * DateTime - encoding/decoding as a string in RFC1123 format.
        /// * MPExt - encoding/decoding of MsgPack Extensions
        /// Also supports encoding/decoding of Nullable<> counterparts of these.
        /// </remarks>

        public bool CanDecode(Type type)
        {
            var t = type;
            if (t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                t = t.GenericTypeArguments[0];

            return t.IsEnum || t == typeof(Guid) || t == typeof(DateTime) || t == typeof(MPExt);
        }

        public bool CanEncode(Type type)
        {
            return CanDecode(type);
        }

        public object Decode(IConverterContext ctx, Type type, MPObject mpo)
        {
            var t = type;
            if (t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (mpo.Type == MPType.Nil)
                    return null;
                t = type.GenericTypeArguments[0];
            }

            if (t.IsEnum)
            {
                if (mpo.Type == MPType.Integer)
                    return Enum.ToObject(t, mpo.Value);
                
                if (mpo.Type == MPType.String)
                    return Enum.Parse(t, (string)mpo.Value);
            }

            if (t == typeof(Guid))
            {
                if (mpo.Type == MPType.String)
                    return Guid.Parse((string)mpo.Value);
                
                if (mpo.Type == MPType.Binary)
                    return new Guid((byte[])mpo.Value);
            }

            if (t == typeof(DateTime))
            {
                if (mpo.Type == MPType.String)
                    return DateTime.Parse((string)mpo.Value);
            }

            if (t == typeof(MPExt))
            {
                if (mpo.Type == MPType.Ext)
                    return (MPExt)mpo.Value;
            }

            throw new MPConversionException(type, mpo);
        }

        public MPObject Encode(IConverterContext ctx, Type type, object obj)
        {
            var t = type;
            if (t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (obj == null)
                    return MPObject.Nil;
                t = type.GenericTypeArguments[0];
            }

            if (t.IsEnum)
            {
                if (EncodeEnumAsNameString)
                    return new MPObject(MPType.String, Enum.GetName(t, obj));
                else
                    return new MPObject(MPType.Integer, (int)obj);
            }

            if (t == typeof(Guid))
            {
                if (EncodeGuidAsString)
                    return new MPObject(MPType.String, ((Guid)obj).ToString());
                else
                    return new MPObject(MPType.Binary, ((Guid)obj).ToByteArray());
            }

            if (t == typeof(DateTime))
            {
                return new MPObject(MPType.String, ((DateTime)obj).ToString("r"));
            }

            if (t == typeof(MPExt))
            {
                return new MPObject(MPType.Ext, (MPExt)obj);
            }

            throw new MPConversionException(type);
        }
    }
}