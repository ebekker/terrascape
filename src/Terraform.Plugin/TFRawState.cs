using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Terraform.Plugin
{
    [JsonConverter(typeof(TFRawStateJsonConverter))]
    public class TFRawState
    {
        private static readonly JsonSerializerOptions _tfOptions = new JsonSerializerOptions
        {
            Converters = {
                Util.TerraformEntityJsonConverterFactory.Instance,
            }
        };

        private byte[] _RawBytes;

        public TFRawState(byte[] rawBytes)
        {
            _RawBytes = rawBytes;
        }

        public ReadOnlyMemory<byte> RawBytes => _RawBytes;        

        public T DeserializeAsJson<T>() => JsonSerializer.Deserialize<T>(_RawBytes, _tfOptions);


        public class TFRawStateJsonConverter : JsonConverter<TFRawState>
        {
            public override TFRawState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new TFRawState(reader.GetBytesFromBase64());
            }

            public override void Write(Utf8JsonWriter writer, TFRawState value, JsonSerializerOptions options)
            {
                writer.WriteBase64StringValue(value.RawBytes.Span);
            }
        }
    }
}
