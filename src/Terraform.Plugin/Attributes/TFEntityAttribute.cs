using System;

namespace Terraform.Plugin.Attributes
{
    /// <summary>
    /// Marker attribute that indicates a type is to be treated as an
    /// entity for the purposes of terraform RPC serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TFEntityAttribute : Attribute
    {
    }
}