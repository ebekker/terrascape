using System;

namespace Terraform.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class TFPluginAttribute : Attribute
    {
        public TFPluginAttribute(Type provider)
        {
            Provider = provider;
        }

        public Type Provider { get; }
    }
}