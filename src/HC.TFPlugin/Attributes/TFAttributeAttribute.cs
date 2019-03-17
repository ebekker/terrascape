using System;

namespace HC.TFPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class TFAttributeAttribute : Attribute
    {
        public TFAttributeAttribute(string name)
        {
            Name = name;
        }
        
        public string Name { get; }

        public string Description { get; set; }

        public bool Computed { get; set; }

        public bool Optional { get; set; }

        public bool Required { get; set; }

        public bool Sensitive { get; set; }

        public bool ForceNew { get; set; }

        public string[] ConflictsWith { get; set; }

        // Type is inferred from the Property type
        //public ByteString Type { get; set; }
    }
}