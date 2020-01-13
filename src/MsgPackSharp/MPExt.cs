using System;
using System.Collections;

namespace MsgPackSharp
{
    public struct MPExt
    {
        public MPExt(int t, ReadOnlyMemory<byte> d)
        {
            Type = t;
            Data = d;
        }

        public readonly int Type;

        public readonly ReadOnlyMemory<byte> Data;

        public override bool Equals(object obj)
        {
            return obj is MPExt ext
                && ext.Type == this.Type
                && ext.Data.Span.SequenceEqual(this.Data.Span);
        }

        public override int GetHashCode()
        {
            int hashCode = -1183445958;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Data.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"{{MPExt({Type},{BitConverter.ToString(Data.ToArray())})}}";
        }
    }
}