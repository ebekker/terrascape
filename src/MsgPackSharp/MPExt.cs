using System;

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
    }
}