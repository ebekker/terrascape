using System;

namespace Terraform.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TFResourceAttribute : TFEntityAttribute
    {
        public TFResourceAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
        
        public long Version { get; set; } = 1L;
    }
}