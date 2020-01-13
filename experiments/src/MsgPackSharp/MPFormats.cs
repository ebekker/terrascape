namespace MsgPackSharp
{
    // https://github.com/msgpack/msgpack/blob/master/spec.md#overview
    internal static class MPFormats
    {
        public static readonly (int start, int end) PositiveFixInt = (0x00, 0x7f);
        public static readonly (int start, int end) NegativeFixInt = (0xe0, 0xff);
        public static readonly (int start, int end) FixMap = (0x80, 0x8f);
        public static readonly (int start, int end) FixArray = (0x90, 0x9f);
        public static readonly (int start, int end) FixStr = (0xa0, 0xbf);
        public const int Nil = 0xc0;
        public const int _NeverUsed_ = 0xc1;
        public const int False = 0xc2;
        public const int True = 0xc3;

        public const int Bin8 = 0xc4;
        public const int Bin16 = 0xc5;
        public const int Bin32 = 0xc6;

        public const int Ext8 = 0xc7;
        public const int Ext16 = 0xc8;
        public const int Ext32 = 0xc9;

        public const int Float32 = 0xca;
        public const int Float64 = 0xcb;
        public const int UInt8 = 0xcc;
        public const int UInt16 = 0xcd;
        public const int UInt32 = 0xce;
        public const int UInt64 = 0xcf;
        public const int Int8 = 0xd0;
        public const int Int16 = 0xd1;
        public const int Int32 = 0xd2;
        public const int Int64 = 0xd3;

        public const int FixExt1 = 0xd4;
        public const int FixExt2 = 0xd5;
        public const int FixExt4 = 0xd6;
        public const int FixExt8 = 0xd7;
        public const int FixExt16 = 0xd8;
        
        public const int Str8 = 0xd9;
        public const int Str16 = 0xda;
        public const int Str32 = 0xdb;

        public const int Array16 = 0xdc;
        public const int Array32 = 0xdd;
        public const int Map16 = 0xde;
        public const int Map32 = 0xdf;

        public static bool In(this int i, (int start, int end) r, bool inclusive = true) => inclusive
            ? i >= r.start && i <= r.end
            : i > r.start && i < r.end;
    }
}