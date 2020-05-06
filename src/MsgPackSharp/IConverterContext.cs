using System;

namespace MsgPackSharp
{
    public interface IConverterContext
    {
        MPObject Encode(Type type, object obj);
        object Decode(Type type, MPObject mpo);
    }
}