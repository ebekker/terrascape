using System;

namespace HC.TFPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TFProviderAttribute : Attribute
    {
        public long Version { get; set; } = 1L;
    }
}