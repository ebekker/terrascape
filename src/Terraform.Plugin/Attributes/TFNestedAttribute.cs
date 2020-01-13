using System;

namespace Terraform.Plugin.Attributes
{

    [AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class TFNestedAttribute : TFArgumentAttribute
    {
        public TFNestedAttribute(string name) : base(name)
        { 
            base.Nested = true;
        }
    }
}