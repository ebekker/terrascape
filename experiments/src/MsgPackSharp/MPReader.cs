using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace MsgPackSharp
{
    // public class MapSerializer : ISerializer
    // {
    //     public object Deserialize(ISerializeContext ctx, Type t, MPObject mpo)
    //     {
    //         if (mpo.Type == MPType.Map)
    //         {
    //             var ht = (IDictionary<MPObject,MPObject>)mpo.Value;

    //             if (t == typeof(IDictionary))
    //             {
    //                 IDictionary map = new Hashtable();
    //                 foreach (var kv in ht)
    //                 {
    //                     var k = ctx.Deserialize(typeof(object), kv.Key);
    //                     var v = ctx.Deserialize(typeof(object), kv.Value);
    //                     map.Add(k, v);
    //                 }

    //                 return map;
    //             }

    //             var mapGtd = typeof(IDictionary<,>);
    //             if (t == mapGtd || (t.IsGenericType && t.GetGenericTypeDefinition() == mapGtd))
    //             {
    //                 var kType = typeof(object);
    //                 var vType = typeof(object);
    //                 if (!t.IsGenericTypeDefinition)
    //                 {
    //                     var gta = t.GenericTypeArguments;
    //                     kType = gta[0];
    //                     vType = gta[1];
    //                 }
    //                 var dictType = typeof(Dictionary<,>).MakeGenericType(kType, vType);
    //                 var dictAdder = dictType.GetMethod("Add", new[] { kType, vType });
    //                 var dict = Activator.CreateInstance(dictType);

    //                 foreach (var kv in ht)
    //                 {
    //                     var k = ctx.Deserialize(kType, kv.Key);
    //                     var v = ctx.Deserialize(vType, kv.Value);
    //                     dictAdder.Invoke(dict, new[] { k, v });
    //                 }
    //             }
    //         }

    //         throw new MPConversionException(t, mpo);
    //     }

    //     public MPObject Serialize(ISerializeContext ctx, Type t, object obj)
    //     {
    //         throw new NotImplementedException();
    //     }
    // }

    // public class MPConverter
    // {
    //     private IDictionary<Type, ISerializer> _serializers = new Dictionary<Type, ISerializer>();

    //     public void Register(Type t, ISerializer ser)
    //     {
    //         _serializers[t] = ser;
    //     }

    //     public object Convert(MPObject mpo, Type type)
    //     {
    //         var ser = ForTypeOrBase(type);
    //         if (ser != null)
    //         {
    //             ISerializeContext ctx = null;
    //             return ser.Deserialize(ctx, type, mpo);
    //         }

    //         return null;
    //     }

    //     public ISerializer ForTypeOrDescendent(Type type)
    //     {
    //         return null;
    //     }

    //     public ISerializer ForTypeOrBase(Type type)
    //     {
    //         for (var t = type; t != null && t != typeof(object); t = t.BaseType)
    //         {
    //             if (_serializers.TryGetValue(t, out var typeSer))
    //                 return typeSer;
                
    //             t = t.BaseType;
    //         }

    //         foreach (var i in type.GetInterfaces())
    //         {
    //             if (_serializers.TryGetValue(i, out var ifaceSer))
    //                 return ifaceSer;
    //         }

    //         for (var t = type; t != null && t != typeof(object); t = t.BaseType)
    //         {
    //             if (t.IsGenericType)
    //             {
    //                 var gtd = t.GetGenericTypeDefinition();
    //                 if (_serializers.TryGetValue(gtd, out var genTypeSer))
    //                     return genTypeSer;
    //             }
                
    //             t = t.BaseType;
    //         }

    //         foreach (var i in type.GetInterfaces())
    //         {
    //             if (i.IsGenericType)
    //             {
    //                 var gtd = i.GetGenericTypeDefinition();
    //                 if (_serializers.TryGetValue(gtd, out var genIfaceSer))
    //                     return genIfaceSer;
    //             }
    //         }
            
    //         return null;
    //     }

    // }

    public class MPReader
    {
        private static readonly ILog _log = Logging.GetLog<MPReader>();

        public static MPObject? Parse(Stream stream)
        {
            var b = stream.ReadByte();
            if (b < 0)
                // EOF
                return null;
            
            if (b.In(MPFormats.PositiveFixInt))
                return new MPObject(MPType.Integer, b - MPFormats.PositiveFixInt.start);

            if (b.In(MPFormats.NegativeFixInt))
                return new MPObject(MPType.Integer, unchecked((long)(b)));
            
            if (b.In(MPFormats.FixStr))
                return new MPObject(MPType.String, Str(stream, b - MPFormats.FixStr.start));

            if (b.In(MPFormats.FixMap))
            {
                var map = Map(stream, b - MPFormats.FixMap.start);
                if (map == null)
                    // Premature EOF
                    return null;
                return new MPObject(MPType.Map, map);
            }

            if (b.In(MPFormats.FixArray))
            {
                var arr = Array(stream, b - MPFormats.FixArray.start);
                if (arr == null)
                    // Premature EOF
                    return null;
                return new MPObject(MPType.Array, arr);
            }

            switch (b)
            {
                case MPFormats._NeverUsed_:
                    throw new InvalidDataException("NeverUsed format encountered");

                case MPFormats.Nil:
                    return MPObject.Nil;

                case MPFormats.False:
                    return MPObject.False;
                case MPFormats.True:
                    return MPObject.True;

                case MPFormats.Bin8:
                    return new MPObject(MPType.Binary, Bin(stream, (int)UInt(stream, 1)));
                case MPFormats.Bin16:
                    return new MPObject(MPType.Binary, Bin(stream, (int)UInt(stream, 2)));
                case MPFormats.Bin32:
                    return new MPObject(MPType.Binary, Bin(stream, (int)UInt(stream, 4)));

                case MPFormats.UInt8:
                    return new MPObject(MPType.Integer, UInt(stream, 1));
                case MPFormats.UInt16:
                    return new MPObject(MPType.Integer, UInt(stream, 2));
                case MPFormats.UInt32:
                    return new MPObject(MPType.Integer, UInt(stream, 4));
                case MPFormats.UInt64:
                    return new MPObject(MPType.Integer, UInt(stream, 8));

                case MPFormats.Int8:
                    return new MPObject(MPType.Integer, Int(stream, 1));
                case MPFormats.Int16:
                    return new MPObject(MPType.Integer, Int(stream, 2));
                case MPFormats.Int32:
                    return new MPObject(MPType.Integer, Int(stream, 4));
                case MPFormats.Int64:
                    return new MPObject(MPType.Integer, Int(stream, 8));

                case MPFormats.Float32:
                    return new MPObject(MPType.Float, Float(stream, 4));
                case MPFormats.Float64:
                    return new MPObject(MPType.Float, Float(stream, 8));

                case MPFormats.Str8:
                    return new MPObject(MPType.String, Str(stream, (int)UInt(stream, 1)));
                case MPFormats.Str16:
                    return new MPObject(MPType.String, Str(stream, (int)UInt(stream, 2)));
                case MPFormats.Str32:
                    return new MPObject(MPType.String, Str(stream, (int)UInt(stream, 4)));

                case MPFormats.Array16:
                    return new MPObject(MPType.Array, ArrayRaw(stream, (int)UInt(stream, 2)));
                case MPFormats.Array32:
                    return new MPObject(MPType.Array, ArrayRaw(stream, (int)UInt(stream, 4)));

                case MPFormats.Map16:
                    //return new MPObject(MPType.Map, MapRaw(stream, (int)UInt(stream, 2)));
                    var map = Map(stream, (int)UInt(stream, 2));
                    if (map == null)
                        // Premature EOF
                        return null;
                    return new MPObject(MPType.Map, map);
                case MPFormats.Map32:
                    return new MPObject(MPType.Map, MapRaw(stream, (int)UInt(stream, 4)));

                case MPFormats.FixExt1:
                    return new MPObject(MPType.Ext, Ext(stream, 1));
                case MPFormats.FixExt2:
                    return new MPObject(MPType.Ext, Ext(stream, 2));
                case MPFormats.FixExt4:
                    return new MPObject(MPType.Ext, Ext(stream, 4));
                case MPFormats.FixExt8:
                    return new MPObject(MPType.Ext, Ext(stream, 8));
                case MPFormats.FixExt16:
                    return new MPObject(MPType.Ext, Ext(stream, 16));

                case MPFormats.Ext8:
                    return new MPObject(MPType.Ext, Ext(stream, (int)UInt(stream, 1)));
                case MPFormats.Ext16:
                    return new MPObject(MPType.Ext, Ext(stream, (int)UInt(stream, 2)));
                case MPFormats.Ext32:
                    return new MPObject(MPType.Ext, Ext(stream, (int)UInt(stream, 4)));

            }

            throw new InvalidDataException($"unknown format 0x{b:X}");
        }

        public static IList<MPObject> Array(Stream stream, int count)
        {
            var arr = new List<MPObject>(count);
            for (int i = 0; i < count; ++i)
            {
                var item = Parse(stream);
                if (item == null)
                    // Premature EOF
                    return null;
                arr.Add(item.Value);
            }
            return arr;
        }

        public static Dictionary<MPObject, MPObject> Map(Stream stream, int count)
        {
            var map = new Dictionary<MPObject, MPObject>();
            for (int i = 0; i < count; ++i)
            {
                var k = Parse(stream);
                if (k == null)
                    return null;
                var v = Parse(stream);
                if (v == null)
                    return null;
                map.Add(k.Value, v.Value);
            }
            return map;
        }

        public static object ParseRaw(Stream stream)
        {
            var b = stream.ReadByte();
            if (b < 0)
                return null;

            if (b.In(MPFormats.PositiveFixInt))
                return "FOO" + (b - MPFormats.PositiveFixInt.start);
            if (b.In(MPFormats.NegativeFixInt))
                return "BAR" + unchecked((sbyte)(b));
            
            if (b.In(MPFormats.FixMap))
                return MapRaw(stream, b - MPFormats.FixMap.start);

            if (b.In(MPFormats.FixArray))
                return ArrayRaw(stream, b - MPFormats.FixArray.start);

            if (b.In(MPFormats.FixStr))
                return Str(stream, b - MPFormats.FixStr.start);

            if (b == MPFormats.Nil)
                return new Nullable<bool>();

            if (b == MPFormats._NeverUsed_)
                throw new InvalidDataException("NeverUsed format encountered");

            if (b == MPFormats.False)
                return false;

            if (b == MPFormats.True)
                return true;

            if (b == MPFormats.Bin8)
                return Bin(stream, (int)UInt(stream, 1));
            
            if (b == MPFormats.Bin16)
                return Bin(stream, (int)UInt(stream, 2));
            
            if (b == MPFormats.Bin32)
                return Bin(stream, (int)UInt(stream, 4));

            if (b == MPFormats.UInt8)
                return UInt(stream, 1);
            if (b == MPFormats.UInt16)
                return UInt(stream, 2);
            if (b == MPFormats.UInt32)
                return UInt(stream, 4);
            if (b == MPFormats.UInt64)
                return UInt(stream, 8);

            if (b == MPFormats.Int8)
                return Int(stream, 1);
            if (b == MPFormats.Int16)
                return Int(stream, 2);
            if (b == MPFormats.Int32)
                return Int(stream, 4);
            if (b == MPFormats.Int64)
                return Int(stream, 8);

            if (b == MPFormats.Float32)
                return Float(stream, 4);
            if (b == MPFormats.Float64)
                return Float(stream, 8);

            if (b == MPFormats.Str8)
                return Str(stream, (int)UInt(stream, 1));
            if (b == MPFormats.Str16)
                return Str(stream, (int)UInt(stream, 2));
            if (b == MPFormats.Str32)
                return Str(stream, (int)UInt(stream, 4));

            if (b == MPFormats.Array16)
                return ArrayRaw(stream, (int)UInt(stream, 2));
            if (b == MPFormats.Array32)
                return ArrayRaw(stream, (int)UInt(stream, 4));

            if (b == MPFormats.Map16)
                return MapRaw(stream, (int)UInt(stream, 2));
            if (b == MPFormats.Map32)
                return MapRaw(stream, (int)UInt(stream, 4));

            if (b == MPFormats.FixExt1)
                return Ext(stream, 1);
            if (b == MPFormats.FixExt2)
                return Ext(stream, 2);
            if (b == MPFormats.FixExt4)
                return Ext(stream, 4);
            if (b == MPFormats.FixExt8)
                return Ext(stream, 8);
            if (b == MPFormats.FixExt16)
                return Ext(stream, 16);

            if (b == MPFormats.Ext8)
                return Ext(stream, (int)UInt(stream, 1));
            if (b == MPFormats.Ext16)
                return Ext(stream, (int)UInt(stream, 2));
            if (b == MPFormats.Ext32)
                return Ext(stream, (int)UInt(stream, 4));

            throw new InvalidDataException($"unknown format 0x{b:X}");
        }

        public static IList ArrayRaw(Stream stream, int count)
        {
            var arr = new ArrayList(count);
            for (int i = 0; i < count; ++i)
                arr.Add(ParseRaw(stream));
            return arr;
        }

        public static IDictionary MapRaw(Stream stream, int count)
        {
            var map = new Hashtable();
            for (int i = 0; i < count; ++i)
                map.Add(ParseRaw(stream), ParseRaw(stream));
            return map;
        }

        public static byte[] Bin(Stream stream, int count, int? pad = null)
        {
            var b = new byte[pad ?? count];
            var o = 0;
            var r = 0;
            while (o < count && (r = stream.Read(b, o, count - o)) > 0)
            {
                o += r;
            }
            if (o < count)
                // Premature EOF
                return null;

            return b;
        }

        public static long Int(Stream stream, int count)
        {
            var b = Bin(stream, count);

            if (count > 0 && BitConverter.IsLittleEndian)
                System.Array.Reverse(b);

            switch (count)
            {
                case 1:
                    return unchecked((sbyte)b[0]);

                case 2:
                    return BitConverter.ToInt16(b, 0);
                
                case 4:
                    return BitConverter.ToInt32(b, 0);

                case 8:
                    return BitConverter.ToInt64(b, 0);
            }

            throw new ArgumentException("invalid count of integer requested", nameof(count));
        }

        public static long UInt(Stream stream, int count)
        {
            var b = Bin(stream, count);

            if (count > 0 && BitConverter.IsLittleEndian)
                System.Array.Reverse(b);

            switch (count)
            {
                case 1:
                    return b[0];

                case 2:
                    return BitConverter.ToUInt16(b, 0);
                
                case 4:
                    return BitConverter.ToUInt32(b, 0);

                case 8:
                    var ul = BitConverter.ToUInt64(b, 0);
                    if (ul > long.MaxValue)
                        // An unfortunate limitation of the way that we store all integer values as [signed] longs
                        throw new Exception("unsigned long value exceeded range of our internal representation");
                    return (long)ul;
            }

            throw new ArgumentException("invalid count of integer requested", nameof(count));
        }

        public static double Float(Stream stream, int count)
        {
            var b = Bin(stream, count);

            switch (count)
            {
                case 4:
                    var f = BitConverter.ToSingle(b, 0);
                    return (double)f;
                
                case 8:
                    var d = BitConverter.ToDouble(b, 0);
                    return d;
            }

            throw new ArgumentException("invalid count of float requested", nameof(count));
        }

        public static string Str(Stream stream, int count)
        {
            return Encoding.UTF8.GetString(Bin(stream, count));
        }

        public static MPExt Ext(Stream stream, int count)
        {
            var t = unchecked((sbyte)stream.ReadByte());
            var b = Bin(stream, count);
            return new MPExt(t, b);
        }
    }
}
