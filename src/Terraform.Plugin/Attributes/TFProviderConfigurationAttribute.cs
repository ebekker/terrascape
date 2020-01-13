using System;

namespace Terraform.Plugin.Attributes
{
    /// <summary>
    /// Used to mark a class as the Provider's configuration who's properties
    /// will be used to derive the schema of the Provider's configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TFProviderConfigurationAttribute : TFEntityAttribute
    {
        public long Version { get; set; } = 1L;
    }
}