using System;

namespace HC.TFPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class TFPluginAttribute : Attribute
    {
        // This is a positional argument
        public TFPluginAttribute(Type provider)
        {
            Provider = provider;
        }

        public Type Provider { get; }        

        public Type[] DataSources { get; set; }

        public Type[] Resources { get; set; }
    }
}