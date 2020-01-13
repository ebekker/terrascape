using System;

namespace HC.TFPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TFDataSourceAttribute : Attribute
    {
        public TFDataSourceAttribute(string name)
        {
            Name = name;
        }
        
        public string Name { get; }

        public long Version { get; set; } = 1L;
    }
}