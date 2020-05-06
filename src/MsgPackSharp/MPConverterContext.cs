using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MsgPackSharp.Converters;

namespace MsgPackSharp
{
    public class MPConverterContext : IConverterContext
    {
        public Collection<IConverter> Converters { get; } =
            new Collection<IConverter>();

        public static IConverterContext CreateDefault()
        {
            var ctx = new MPConverterContext();
            ctx.Converters.Add(BasicConverter.Instance);
            ctx.Converters.Add(CommonConverter.DefaultInstance);
            ctx.Converters.Add(MapConverter.Instance);
            ctx.Converters.Add(ReadOnlyMapConverter.Instance);
            ctx.Converters.Add(ArrayConverter.Instance);
            ctx.Converters.Add(ObjectConverter.DefaultInstance);
            ctx.Converters.Add(DefaultConverter.Instance);
            return ctx;
        }

        public static IConverterContext CreateDefault(
            IEnumerable<IConverter> objectConverters,
            bool replaceDefaultObjectConverter = false)
        {
            var ctx = new MPConverterContext();
            ctx.Converters.Add(BasicConverter.Instance);
            ctx.Converters.Add(CommonConverter.DefaultInstance);
            ctx.Converters.Add(MapConverter.Instance);
            ctx.Converters.Add(ReadOnlyMapConverter.Instance);
            ctx.Converters.Add(ArrayConverter.Instance);

            foreach (var c in objectConverters)
                ctx.Converters.Add(c);
            if (!replaceDefaultObjectConverter)
                ctx.Converters.Add(ObjectConverter.DefaultInstance);

            ctx.Converters.Add(DefaultConverter.Instance);
            return ctx;
        }

        // Do we need these???

        // public MPObject Encode<T>(T obj) => Encode(typeof(T), obj);

        // public T Decode<T>(MPObject mpo) => (T)Decode(typeof(T), mpo);

        public virtual MPObject Encode(Type type, object obj)
        {
            foreach (var c in Converters)
            {
                if (c.CanEncode(type))
                    return c.Encode(this, type, obj);
            }

            throw new MPConversionException(type,
                message: $"found no matching encoder for target type [{type.FullName}]");
        }

        public virtual object Decode(Type type, MPObject mpo)
        {
            foreach (var c in Converters)
            {
                if (c.CanDecode(type))
                    return c.Decode(this, type, mpo);
            }

            throw new MPConversionException(type, mpo,
                message: $"found no matching decoder for target type [{type.FullName}]");
        }
    }
}