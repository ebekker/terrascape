using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Terraform.Plugin
{
    [JsonConverter(typeof(TFResourceChangeTypeJsonConverter))]
    public enum TFResourceChangeType
    {
        Unknown = 0,
        Create = 1,
        Update = 2,
        Delete = 3,
    }

    public class TFResourceChangeTypeJsonConverter : JsonConverter<TFResourceChangeType>
    {
        public override TFResourceChangeType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TryGetInt64(out var value))
            {
                return (TFResourceChangeType)value;
            }
            else
            {
                return Enum.Parse<TFResourceChangeType>(reader.GetString(), true);
            }
        }

        public override void Write(Utf8JsonWriter writer, TFResourceChangeType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Enum.GetName(typeof(TFResourceChangeType), value));
        }
    }
}