using System;
using System.Reflection;

namespace HC.TFPlugin.Attributes
{
    public abstract class TFAttributeAttribute : Attribute
    {
        public TFAttributeAttribute(string name)
        {
            Name = name;
        }
        
        public string Name { get; }

        public string Description { get; set; }

        public bool Sensitive { get; set; }

        public bool Optional { get; protected set; }

        public bool Required { get; protected set; }

        public bool ForceNew { get; protected set; }

        public string[] ConflictsWith { get; protected set; }

        public bool Computed { get; protected set; }

        // Type is inferred from the Property type
        //public ByteString Type { get; set; }

        public static TFAttributeAttribute Get<T>(string propName) =>
            typeof(T).GetProperty(propName)?.GetCustomAttribute<TFAttributeAttribute>();
    }
}