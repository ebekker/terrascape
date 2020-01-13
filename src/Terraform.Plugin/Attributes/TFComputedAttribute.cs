using System;

namespace Terraform.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class TFComputedAttribute : TFAttributeAttribute
    {
        public TFComputedAttribute(string name) : base(name)
        {
            base.Computed = true;
        }
    }
}