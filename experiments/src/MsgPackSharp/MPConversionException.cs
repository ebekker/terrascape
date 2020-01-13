using System;

namespace MsgPackSharp
{
    public class MPConversionException : Exception
    {
        public MPConversionException(Type type, MPObject? mpo = null,
            string message = null, Exception inner = null) : base(message, inner)
        {
            TargetType = type;
            MPObject = mpo;
        }

        public Type TargetType { get; }

        public MPObject? MPObject { get; }
    }
}