using System;

namespace Terraform.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TFDataSourceAttribute : TFEntityAttribute
    {
        public TFDataSourceAttribute(string name)
        {
            Name = name;
        }
        
        public string Name { get; }

        public long Version { get; set; } = 1L;
    }
}