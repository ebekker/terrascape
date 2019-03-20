using System;

namespace HC.TFPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class TFArgumentAttribute: TFAttributeAttribute
    {
        public TFArgumentAttribute(string name) : base(name, false)
        { }
    }
}