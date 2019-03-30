namespace MsgPackSharp
{
    public struct MPObject
    {
        public static readonly MPObject Nil = new MPObject(MPType.Nil, null);
        public static readonly MPObject False = new MPObject(MPType.Boolean, false);
        public static readonly MPObject True = new MPObject(MPType.Boolean, true);
        public static readonly MPObject ZeroInteger = new MPObject(MPType.Integer, 0);
        public static readonly MPObject EmptyString = new MPObject(MPType.String, string.Empty);
        public static readonly MPObject EmptyBinary = new MPObject(MPType.Binary, new byte[0]);

        public MPObject(MPType t, object v)
        {
            Type = t;
            Value = v;
        }

        public readonly MPType Type;
        public readonly object Value;

        public override string ToString() => $"MPObject {{ Type = {Type}, Value = {Value} }}";
    }
}