using System;
using System.Collections.Generic;
using System.Reflection;
using Terraform.Plugin.Attributes;
using Tfplugin5;

namespace Terraform.Plugin
{
    public interface ISchemaDetails
    {
        string Name { get; }

        Type Type { get; }

        Attribute Attribute { get; }

        Schema Schema { get; }
    }

    public interface ISchemaResolver
    {
        Assembly PluginAssembly { get; }

        TFPluginAttribute PluginDetails { get; }

        ISchemaDetails GetProviderConfigurationSchema();

        IReadOnlyDictionary<string, ISchemaDetails> GetDataSourceSchemas();

        IReadOnlyDictionary<string, ISchemaDetails> GetResourceSchemas();
    }
}
