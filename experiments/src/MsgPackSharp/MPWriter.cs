using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MsgPackSharp
{
    public class MPWriter
    {
        public static readonly byte[] NoBytes = new byte[0];

        private Stream _stream;

        // This is a reusable byte buffer used by various methods instead of
        // reallocating a new buffer of various small sized over and over
        // NOTE:  clearly this type is *NOT* reentrant-safe
        private byte[] _bytes = new byte[16];

        public MPWriter(Stream stream)
        {
            _stream = stream;
        }

        public int Emit(MPObject mpo)
        {
            System.Array.Clear(_bytes, 0, _bytes.Length);
            switch (mpo.Type)
            {
                case MPType.Nil:
                    _bytes[0] = MPFormats.Nil;
                    _stream.Write(_bytes, 0, 1);
                    return 1;

                case MPType.Boolean:
                    _bytes[0] = (byte)(((bool)mpo.Value) ? MPFormats.True : MPFormats.False);
                    _stream.Write(_bytes, 0, 1);
                    return 1;

                case MPType.Integer:
                    return Int(mpo);

                case MPType.Float:
                    return Float(mpo);

                case MPType.Binary:
                    return Bin(mpo);

                case MPType.String:
                    return Str(mpo);

                case MPType.Array:
                    return Array(mpo);

                case MPType.Map:
                    return Map(mpo);

                case MPType.Ext:
                    return Ext(mpo);
            }

            throw new Exception("Can't emit byte stream for MPO type: " + mpo.Type);
        }

        public int Ext(MPObject mpo)
        {
            var ext = (MPExt)mpo.Value;
            var extDataLen = ext.Data.Length;
            byte fmt;
            byte[] nb;

            if (extDataLen < byte.MaxValue)
            {
                nb = NoBytes;
                switch (extDataLen)
                {
                    // Handle the special cases for specific data lengths...
                    case 1: fmt = MPFormats.FixExt1; break;
                    case 2: fmt = MPFormats.FixExt2; break;
                    case 4: fmt = MPFormats.FixExt4; break;
                    case 8: fmt = MPFormats.FixExt8; break;
                    case 16: fmt = MPFormats.FixExt16; break;

                    // ...then default to the general var-length case
                    default:
                        fmt = MPFormats.Ext8;
                        nb = new[] { unchecked((byte)extDataLen) };
                        break;
                }
            }
            else if (extDataLen < ushort.MaxValue)
            {
                fmt = MPFormats.Ext16;
                nb = BitConverter.GetBytes((ushort)extDataLen);
            }
            // This should be uint.MaxValue but Span<> can't store that
            // many elements or at least can't give us a count that high
            else if (extDataLen < int.MaxValue)
            {
                fmt = MPFormats.Ext32;
                nb = BitConverter.GetBytes((uint)extDataLen);
            }
            else
            {
                throw new Exception("Can't emit byte stream for MPO type: " + mpo.Type);
            }

            // Ensure network byte order
            if (extDataLen >= byte.MaxValue && BitConverter.IsLittleEndian)
                System.Array.Reverse(nb);

            _stream.WriteByte(fmt);
            _stream.Write(nb, 0, nb.Length);
            _stream.WriteByte((byte)ext.Type);
            int written = 1 + nb.Length + 1;
            _stream.Write(ext.Data.ToArray(), 0, extDataLen);
            return written;
        }

        public int Map(MPObject mpo)
        {
            var map = (IDictionary<MPObject, MPObject>)mpo.Value;
            var mapCount = map.Count;
            byte fmt;
            byte[] nb;

            if (mapCount <= 15)
            {
                fmt = unchecked((byte)(mapCount + MPFormats.FixMap.start));
                nb = NoBytes;
            }
            else if (mapCount < ushort.MaxValue)
            {
                fmt = MPFormats.Map16;
                nb = BitConverter.GetBytes((ushort)mapCount);
            }
            // This should be uint.MaxValue but Dictionary can't store that
            // many elements or at least can't give us a count that high
            else if (mapCount < int.MaxValue)
            {
                fmt = MPFormats.Map32;
                nb = BitConverter.GetBytes((uint)mapCount);
            }
            else
            {
                throw new Exception("Can't emit byte stream for MPO type: " + mpo.Type);
            }

            // Ensure network byte order
            if (mapCount > 16 && BitConverter.IsLittleEndian)
                System.Array.Reverse(nb);

            _stream.WriteByte(fmt);
            _stream.Write(nb, 0, nb.Length);
            int written = 1 + nb.Length;
            foreach (var kv in map)
            {
                written += Emit(kv.Key);
                written += Emit(kv.Value);
            }
            return written;
        }

        public int Array(MPObject mpo)
        {
            var arr = (IList<MPObject>)mpo.Value;
            var arrCount = arr.Count;
            byte fmt;
            byte[] nb;

            if (arrCount <= 15)
            {
                fmt = unchecked((byte)(arrCount + MPFormats.FixArray.start));
                nb = NoBytes;
            }
            else if (arrCount < ushort.MaxValue)
            {
                fmt = MPFormats.Array16;
                nb = BitConverter.GetBytes((ushort)arrCount);
            }
            // This should be uint.MaxValue but List can't store that
            // many elements or at least can't give us a count that high
            else if (arrCount < int.MaxValue)
            {
                fmt = MPFormats.Array32;
                nb = BitConverter.GetBytes((uint)arrCount);
            }
            else
            {
                throw new Exception("Can't emit byte stream for MPO type: " + mpo.Type);
            }

            // Ensure network byte order
            if (arrCount > 16 && BitConverter.IsLittleEndian)
                System.Array.Reverse(nb);

            _stream.WriteByte(fmt);
            _stream.Write(nb, 0, nb.Length);
            int written = 1 + nb.Length;
            foreach (var item in arr)
            {
                written += Emit(item);
            }
            return written;
        }

        public int Str(MPObject mpo)
        {
            var s = (string)mpo.Value;
            var b = Encoding.UTF8.GetBytes(s);
            byte fmt;
            byte[] nb;

            if (b.Length <= byte.MaxValue)
            {
                fmt = MPFormats.Str8;
                nb = new byte[] { unchecked((byte)b.Length) };
            }
            else if (b.Length <= ushort.MaxValue)
            {
                fmt = MPFormats.Str16;
                nb = BitConverter.GetBytes((ushort)b.Length);
                    
            }
            else if (b.LongLength <= uint.MaxValue)
            {
                fmt = MPFormats.Str32;
                nb = BitConverter.GetBytes((uint)b.Length);
            }
            else
            {
                throw new Exception("Can't emit byte stream for MPO type: " + mpo.Type);
            }

            // Ensure network byte order
            if (b.Length > byte.MaxValue && BitConverter.IsLittleEndian)
                System.Array.Reverse(nb);

            _stream.WriteByte(fmt);
            _stream.Write(nb, 0, nb.Length);
            _stream.Write(b, 0, b.Length);

            return 1 + nb.Length + b.Length;
        }

        public int Bin(MPObject mpo)
        {
            var b = (byte[])mpo.Value;
            byte fmt;
            byte[] nb;

            if (b.Length <= byte.MaxValue)
            {
                fmt = MPFormats.Bin8;
                nb = new byte[] { unchecked((byte)b.Length) };
            }
            else if (b.Length <= ushort.MaxValue)
            {
                fmt = MPFormats.Bin16;
                nb = BitConverter.GetBytes((ushort)b.Length);
            }
            else if (b.LongLength <= uint.MaxValue)
            {
                fmt = MPFormats.Bin32;
                nb = BitConverter.GetBytes((uint)b.Length);
            }
            else
            {
                throw new Exception("Can't emit byte stream for MPO type: " + mpo.Type);
            }

            // Ensure network byte order
            if (b.Length > byte.MaxValue && BitConverter.IsLittleEndian)
                System.Array.Reverse(nb);

            _stream.WriteByte(fmt);
            _stream.Write(nb, 0, nb.Length);
            _stream.Write(b, 0, b.Length);

            return 1 + nb.Length + b.Length;
        }

        public int Float(MPObject mpo)
        {
            switch (mpo.Value)
            {
                case float f:
                    var fb = BitConverter.GetBytes(f);
                    _stream.WriteByte(MPFormats.Float32);
                    _stream.Write(fb, 0, fb.Length);
                    return 1 + fb.Length;

                case double d:
                    var db = BitConverter.GetBytes(d);
                    _stream.WriteByte(MPFormats.Float64);
                    _stream.Write(db, 0, db.Length);
                    return 1 + db.Length;
            }

            throw new Exception("Can't emit byte stream for MPO type: " + mpo.Type);
        }

        public int Int(MPObject mpo)
        {
            byte fmt;
            byte[] nb;

            switch (mpo.Value)
            {
                case sbyte n:
                    fmt = MPFormats.Int8;
                    nb = new[] { unchecked((byte)n) };
                    break;
                case byte n:
                    fmt = MPFormats.UInt8;
                    nb = new[] { n };
                    break;
                case short n:
                    fmt = MPFormats.Int16;
                    nb = BitConverter.GetBytes(n);
                    break;
                case ushort n:
                    fmt = MPFormats.UInt16;
                    nb = BitConverter.GetBytes(n);
                    break;
                case int n:
                    fmt = MPFormats.Int32;
                    nb = BitConverter.GetBytes(n);
                    break;
                case uint n:
                    fmt = MPFormats.UInt32;
                    nb = BitConverter.GetBytes(n);
                    break;
                case long n:
                    fmt = MPFormats.Int64;
                    nb = BitConverter.GetBytes(n);
                    break;
                case ulong n:
                    fmt = MPFormats.UInt64;
                    nb = BitConverter.GetBytes(n);
                    break;

                default:
                    throw new Exception("Can't emit byte stream for MPO type: " + mpo.Type);
            }

            _stream.WriteByte(fmt);
            _stream.Write(nb, 0, nb.Length);
            return nb.Length;
        }
    }
}