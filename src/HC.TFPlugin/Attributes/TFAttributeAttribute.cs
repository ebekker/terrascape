using System;
using System.Reflection;

namespace HC.TFPlugin.Attributes
{
    public abstract class TFAttributeAttribute : Attribute
    {
        public TFAttributeAttribute(string name, bool computed)
        {
            Name = name;
            Computed = computed;
        }
        
        public string Name { get; }

        public bool Computed { get; }

        public string Description { get; set; }

        public bool Optional { get; set; }

        public bool Required { get; set; }

        public bool Sensitive { get; set; }

        public bool ForceNew { get; set; }

        public string[] ConflictsWith { get; set; }

        // Type is inferred from the Property type
        //public ByteString Type { get; set; }

        public static TFAttributeAttribute Get<T>(string propName) =>
            typeof(T).GetProperty(propName)?.GetCustomAttribute<TFAttributeAttribute>();
    }
}