using System;
using Microsoft.Extensions.Logging;
using Tfplugin5;

namespace Terraform.Plugin.Util
{
    // Special Support for go-cty "Unknown Values":
    //  https://github.com/zclconf/go-cty/blob/master/cty/msgpack/doc.go
    //  https://github.com/zclconf/go-cty/blob/master/cty/msgpack/unknown.go

    public partial class DynamicValueExtensions
    {
        public static DynamicValue MarshalToDynamicValue<T>(this T value,
            bool withUnknowns = false)
        {
            return MarshalToDynamicValue(value, typeof(T), withUnknowns);
        }
        
        public static DynamicValue MarshalToDynamicValue(this object value, Type t,
            bool withUnknowns = false)
        {
            return MarshalViaMPSharp(t, value, withUnknowns);
        }

        public static T UnmarshalFromDynamicValue<T>(this DynamicValue value)
        {
            return (T)UnmarshalFromDynamicValue(value, typeof(T));
        }

        public static object UnmarshalFromDynamicValue(this DynamicValue value, Type t)
        {
            // return HC.TFPlugin.DynamicValueExtensions.UnmarshalViaMsgPackCli(t, value);
            return UnmarshalViaMPSharp(t, value);
        }
    }
}