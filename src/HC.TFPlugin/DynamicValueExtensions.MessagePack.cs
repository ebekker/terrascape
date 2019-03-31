using System;
using Google.Protobuf;
using MessagePack;
using Newtonsoft.Json;
using Tfplugin5;

namespace HC.TFPlugin
{
    // Special Support for go-cty "Unknown Values":
    //  https://github.com/zclconf/go-cty/blob/master/cty/msgpack/doc.go
    //  https://github.com/zclconf/go-cty/blob/master/cty/msgpack/unknown.go

    public static partial class DynamicValueExtensions
    {
        // https://github.com/zclconf/go-cty/blob/master/cty/msgpack/unknown.go
        private static readonly byte[] UnknownValBytes = new byte[] { 0xd4, 0, 0 };

        // These are kinda inefficient -- the use JSON encoding as an
        // intermediary between MsgPack and native objects for both
        // serializing and deserializing because that gives us a way
        // to control custom serialization behavior more easily and
        // with more flexibility (i.e. based on custom attributes,
        // custom naming logic, etc.)
        //
        // This is TEMPORARY until we can focus on implementing the
        // same behavior direclty in MsgPack encoding/decoding

        public static object UnmarshalViaJson(Type t, DynamicValue dv) =>
            DeserializeMsgPackViaJson(t, dv.Msgpack.ToByteArray());

        public static object DeserializeMsgPackViaJson(Type t, byte[] b) =>
            JsonConvert.DeserializeObject(MessagePackSerializer.ToJson(b), t,
                TFCustomContractResolver.SerializationSettings);

        public static DynamicValue MarshalViaJson(Type t, object value) =>
            new DynamicValue
            {
                Msgpack = ByteString.CopyFrom(SerializeMsgPackViaJson(t, value)),
            };

        public static byte[] SerializeMsgPackViaJson(Type t, object value) =>
            MessagePackSerializer.FromJson(JsonConvert.SerializeObject(value, t,
                TFCustomContractResolver.SerializationSettings));



        public static T UnmarshalViaMessagePack<T>(DynamicValue dv, T def = default) =>
            DeserializeMsgPack<T>(dv.Msgpack.ToByteArray());

        public static object UnmarshalViaMessagePack(Type t, DynamicValue dv) =>
            DeserializeMsgPack(t, dv.Msgpack.ToByteArray());

        public static DynamicValue MarshalViaMessagePack<T>(T value) =>
            new DynamicValue
            {
                Msgpack = ByteString.CopyFrom(SerializeMsgPack(value)),
            };

        public static DynamicValue MarshalViaMessagePack(Type t, object value) =>
            new DynamicValue
            {
                Msgpack = ByteString.CopyFrom(SerializeMsgPack(t, value)),
            };

        public static T DeserializeMsgPack<T>(byte[] b, T def = default) =>
            MessagePackSerializer.Deserialize<T>(b,
                MessagePack.Resolvers.ContractlessStandardResolver.Instance);

        public static object DeserializeMsgPack(Type t, byte[] b) =>
            MessagePackSerializer.NonGeneric.Deserialize(t, b,
                MessagePack.Resolvers.ContractlessStandardResolver.Instance);

        public static byte[] SerializeMsgPack<T>(T value) =>
            MessagePackSerializer.Serialize<T>(value,
                MessagePack.Resolvers.ContractlessStandardResolver.Instance);

        public static byte[] SerializeMsgPack(Type t, object value) =>
            MessagePackSerializer.NonGeneric.Serialize(t, value,
                MessagePack.Resolvers.ContractlessStandardResolver.Instance);
    }
}