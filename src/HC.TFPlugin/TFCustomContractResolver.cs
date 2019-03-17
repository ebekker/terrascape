using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HC.TFPlugin.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HC.TFPlugin
{
    public class TFCustomContractResolver : DefaultContractResolver
    {
        public static readonly TFCustomContractResolver Instance = new TFCustomContractResolver();

        public static readonly JsonSerializerSettings SerializationSettings = new JsonSerializerSettings
        {
            ContractResolver = TFCustomContractResolver.Instance,
        };

        public TFCustomContractResolver()
        { }

        protected override IList<JsonProperty> CreateProperties(Type type,
            MemberSerialization memberSerialization) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => (prop: p, attr: p.GetCustomAttribute<TFAttributeAttribute>()))
                .Where(pa => pa.attr != null)
                .Select(pa => new JsonProperty
                {
                    PropertyType = pa.prop.PropertyType,
                    PropertyName = pa.attr.Name,
                    Readable = true,
                    Writable = true,
                    ValueProvider = CreateMemberValueProvider(pa.prop),
                }).ToList();
    }
}