using System;
using Google.Protobuf;
using MessagePack;
using Newtonsoft.Json;
using Tfplugin5;

namespace HC.TFPlugin
{
    public class DVHelper
    {
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



        public static T Unmarshal<T>(DynamicValue dv, T def = default) =>
            DeserializeMsgPack<T>(dv.Msgpack.ToByteArray());

        public static object Unmarshal(Type t, DynamicValue dv) =>
            DeserializeMsgPack(t, dv.Msgpack.ToByteArray());

        public static DynamicValue Marshal<T>(T value) =>
            new DynamicValue
            {
                Msgpack = ByteString.CopyFrom(SerializeMsgPack(value)),
            };

        public static DynamicValue Marshal(Type t, object value) =>
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