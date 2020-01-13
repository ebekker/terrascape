using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Terraform.Plugin.Attributes;

namespace Terraform.Plugin.Util
{
    // This is derived from the example factory approach in:
    //  https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to#sample-factory-pattern-converter

    public class TerraformEntityJsonConverterFactory : JsonConverterFactory
    {
        public static readonly TerraformEntityJsonConverterFactory Instance =
            new TerraformEntityJsonConverterFactory();

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert
                .GetCustomAttributes(typeof(TFEntityAttribute), false)?.Length > 0;
        }

        public override JsonConverter CreateConverter(
            Type type, 
            JsonSerializerOptions options)
        {

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(TerraformEntityJsonConverter<>).MakeGenericType(type),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);

            return converter;
        }

        private class TerraformEntityJsonConverter<T> : JsonConverter<T> where T : class, new()
        {
            private Type _type;

            public TerraformEntityJsonConverter(JsonSerializerOptions options)
            {
                // // For performance, use the existing converter if available.
                // _valueConverter = (JsonConverter<TValue>)options
                //     .GetConverter(typeof(TValue));

                // Cache the key and value types.
                _type = typeof(T);
            }

            public override T Read(
                ref Utf8JsonReader reader, 
                Type typeToConvert, 
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                var entity = new T();
                var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(p => (prop: p, attr: p.GetCustomAttribute<TFAttributeAttribute>()))
                    .Where(pa => pa.attr != null)
                    .ToDictionary(pa => pa.attr.Name, pa => pa.prop);

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return entity;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    var propName = reader.GetString();
                    if (!props.TryGetValue(propName, out var prop))
                    {
                        // Discard the property
                        reader.Read();
                        continue;
                    }

                    var propValue = JsonSerializer.Deserialize(ref reader, prop.PropertyType, options);
                    prop.SetValue(entity, propValue);
                }

                throw new JsonException();
            }

            public override void Write(
                Utf8JsonWriter writer, 
                T value, 
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                foreach (var prop in typeof(T)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var attrs = prop.GetCustomAttributes(typeof(TFAttributeAttribute), false);
                    if (attrs?.Length == 0)
                        continue;
                    
                    var tfAttr = (TFAttributeAttribute)attrs[0];
                    writer.WritePropertyName(tfAttr.Name);
                    var propValue = prop.GetValue(value);
                    JsonSerializer.Serialize(writer, propValue, options);
                }

                writer.WriteEndObject();
            }
        }
    }
}
