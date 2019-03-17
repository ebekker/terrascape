using System;

namespace HC.TFPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TFResourceAttribute : Attribute
    {
        public TFResourceAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
        
        public long Version { get; set; } = 1L;
    }
}