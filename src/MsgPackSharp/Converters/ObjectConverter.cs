using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MsgPackSharp.Converters
{
    public class ObjectConverter : IConverter
    {
        public static readonly IPropertyNamesResolver DefaultPropertyNamesResolver =
            new SimplePropertyNamesResolver();

        public static readonly ObjectConverter DefaultInstance = new ObjectConverter();

        public ObjectConverter(IPropertyNamesResolver resolver = null)
        {
            Resolver = resolver ?? DefaultPropertyNamesResolver;
        }

        public IPropertyNamesResolver Resolver { get; }

        public bool CanDecode(Type type)
        {
            return !type.IsValueType && type.GetConstructor(Type.EmptyTypes) != null;
        }

        public bool CanEncode(Type type)
        {
            return CanDecode(type);
        }

        public object Decode(IConverterContext ctx, Type type, MPObject mpo)
        {
            if (mpo.Type == MPType.Nil)
                return null;

            if (mpo.Type == MPType.Map)
            {
                var map = (IDictionary<MPObject, MPObject>)mpo.Value;
                var obj = Activator.CreateInstance(type);

                var props = Resolver.ResolvePropertyNames(type);

                foreach (var kv in map)
                {
                    if (kv.Key.Type != MPType.String)
                        throw new MPConversionException(type, mpo,
                            message: "can't resolve property from non-string key");
                                        
                    var propName = (string)kv.Key.Value;
                    if (!props.TryGetValue(propName, out var prop))
                        throw new MPConversionException(type, mpo,
                            message: $"can't resolve property from name [{propName}]");
                    
                    var value = ctx.Decode(prop.PropertyType, kv.Value);
                    prop.SetValue(obj, value);
                }

                return obj;
            }

            throw new MPConversionException(type, mpo);
        }

        public MPObject Encode(IConverterContext ctx, Type type, object obj)
        {
            if (obj == null)
                return MPObject.Nil;
            
            var map = new Dictionary<MPObject, MPObject>();
            var props = Resolver.ResolvePropertyNames(type);
            foreach (var prop in props)
            {
                var propInfo = prop.Value;
                if (propInfo.GetIndexParameters()?.Length > 0)
                    // Skip indexer properties
                    continue;

                var propName = ctx.Encode(typeof(string), prop.Key);
                var propType = propInfo.PropertyType;
                var propValue = ctx.Encode(propType, propInfo.GetValue(obj));
                map.Add(propName, propValue);
            }
            return new MPObject(MPType.Map, map);
        }

        public interface IPropertyNamesResolver
        {
            IReadOnlyDictionary<string, PropertyInfo> ResolvePropertyNames(Type type);
        }

        internal class SimplePropertyNamesResolver : IPropertyNamesResolver
        {
            public IReadOnlyDictionary<string, PropertyInfo> ResolvePropertyNames(Type type)
            {
                return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .ToDictionary(p => p.Name, p => p);
            }
        }
    }
}