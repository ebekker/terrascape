
using System;

namespace Tfplugin5
{
    public partial class DynamicValue
    {
        public static DynamicValue Marshal<T>(T value)
        {
            return Marshal(typeof(T), value);
        }
        
        public static DynamicValue Marshal(Type t, object value)
        {
            return HC.TFPlugin.DynamicValueExtensions.MarshalViaMsgPackCli(t, value);
        }

        public static T Unmarshal<T>(DynamicValue value)
        {
            return (T)Unmarshal(typeof(T), value);
        }

        public static object Unmarshal(Type t, DynamicValue value)
        {
            return HC.TFPlugin.DynamicValueExtensions.UnmarshalViaMsgPackCli(t, value);
        }
    } 
}