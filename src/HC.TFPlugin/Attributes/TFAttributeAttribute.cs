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

        /// <summary>
        /// Description is used as the description for docs or asking for user
        /// input. It should be relatively short (a few sentences max) and should
        /// be formatted to fit a CLI.
        /// </summary>
        /// <see cref="https://github.com/hashicorp/terraform/blob/master/helper/schema/schema.go#L169" />
        public string Description { get; set; }

        public bool Sensitive { get; set; }

        public bool Nested { get; protected set; }

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