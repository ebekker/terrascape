using System;

namespace MsgPackSharp
{
    public interface IConverter
    {
        bool CanEncode(Type type);
        bool CanDecode(Type type);

        MPObject Encode(IConverterContext ctx, Type type, object obj);
        object Decode(IConverterContext ctx, Type type, MPObject mpo);
    }
}