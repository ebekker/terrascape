using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Terraform.Plugin.Attributes;
using Terraform.Plugin.Util;
using Tfplugin5;

namespace Terraform.Plugin.Services
{
    public class SchemaResolver : ISchemaResolver
    {
        private readonly ILogger _log;

        private SchemaDetails _providerConfigurationSchema;
        private IReadOnlyDictionary<string, ISchemaDetails> _dataSourceSchemas;
        private IReadOnlyDictionary<string, ISchemaDetails> _resourceSchemas;

        public SchemaResolver(ILogger<SchemaResolver> log)
        {
            _log = log;

            PluginAssembly = Assembly.GetEntryAssembly();
            PluginDetails = PluginAssembly.GetCustomAttribute<TFPluginAttribute>();

            _log.LogInformation("SchemaResolver constructed and ready");
        }

        public Assembly PluginAssembly { get; }

        public TFPluginAttribute PluginDetails { get; }

        public ISchemaDetails GetProviderConfigurationSchema()
        {
            if (_providerConfigurationSchema == null)
            {
                // We expect at most 1 type to be discovered here
                var configTypes = PluginAssembly.GetExportedTypes()
                    .Select(type => new
                    {
                        Type = type,
                        Attr = type.GetCustomAttribute<TFProviderConfigurationAttribute>()
                    })
                    .Where(t_a => t_a.Attr != null)
                    .ToArray();

                // Only a single class may be marked as *the* Provider configuration
                if (configTypes.Length > 1)
                    throw new InvalidOperationException(
                        "Multiple provider configuration types discovered, at most one supported");

                _providerConfigurationSchema = new SchemaDetails
                {
                    Name = string.Empty,
                };

                if (configTypes.Length == 1)
                {
                    _providerConfigurationSchema.Type = configTypes[0].Type;
                    _providerConfigurationSchema.Attribute = configTypes[0].Attr;
                }
                else
                {
                    _log.LogWarning("No provider configuration type found, assuming no configuration");
                    _providerConfigurationSchema.Type = typeof(object);
                    _providerConfigurationSchema.Attribute = new TFProviderConfigurationAttribute();
                }
                _log.LogInformation("Resolved provider configuration type [{type}]",
                    _providerConfigurationSchema.Type);

                _providerConfigurationSchema.Schema = GetSchemaFromType<TFProviderConfigurationAttribute>(
                    _providerConfigurationSchema.Type, (type, attr) => {
                        return new Schema
                        {
                            Version = attr.Version,
                            Block = new Schema.Types.Block(),
                        };
                    });

            }
            return _providerConfigurationSchema;
        }

        public IReadOnlyDictionary<string, ISchemaDetails> GetDataSourceSchemas()
        {
            if (_dataSourceSchemas == null)
            {
                _dataSourceSchemas = PluginAssembly.GetExportedTypes()
                    .Select(type => new
                    {
                        Type = type,
                        Attr = type.GetCustomAttribute<TFDataSourceAttribute>()
                    })
                    .Where(t_a => t_a.Attr != null)
                    .ToDictionary(t_a => t_a.Attr.Name,
                        t_a => ((ISchemaDetails)new SchemaDetails
                        {
                            Name = t_a.Attr.Name,
                            Type = t_a.Type,
                            Attribute = t_a.Attr,
                            Schema = GetSchemaFromType<TFDataSourceAttribute>(
                            t_a.Type, (type, attr) => new Schema
                            {
                                Version = t_a.Attr.Version,
                                Block = new Schema.Types.Block(),
                            }),
                        }));
                                    }
            return _dataSourceSchemas;
        }

        public IReadOnlyDictionary<string, ISchemaDetails> GetResourceSchemas()
        {
            if (_resourceSchemas == null)
            {
                _resourceSchemas = PluginAssembly.GetExportedTypes()
                    .Select(type => new
                    {
                        Type = type,
                        Attr = type.GetCustomAttribute<TFResourceAttribute>()
                    })
                    .Where(t_a => t_a.Attr != null)
                    .ToDictionary(t_a => t_a.Attr.Name,
                        t_a => ((ISchemaDetails)new SchemaDetails
                        {
                            Name = t_a.Attr.Name,
                            Type = t_a.Type,
                            Attribute = t_a.Attr,
                            Schema = GetSchemaFromType<TFResourceAttribute>(
                            t_a.Type, (type, attr) => new Schema
                            {
                                Version = t_a.Attr.Version,
                                Block = new Schema.Types.Block(),
                            }),
                        }));
            }
            return _resourceSchemas;
        }

        private Schema GetSchemaFromType<TAttr>(Type type, Func<Type, TAttr, Schema> schemaCreator)
            where TAttr : Attribute
        {
            var schema = schemaCreator(type, type.GetCustomAttribute<TAttr>());
            AddAttrs(schema.Block, type);
            return schema;
        }

        private Schema.Types.Block AddAttrs(Schema.Types.Block block, Type t)
        {
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attrAttr = p.GetCustomAttribute<TFAttributeAttribute>();
                if (attrAttr == null)
                    continue;
                
                var propType = p.PropertyType;
                
                if (attrAttr.Nested)
                {
                    block.BlockTypes.Add(AddNested(p));
                }
                else
                {
                    block.Attributes.Add(new Schema.Types.Attribute
                    {
                        Name = attrAttr.Name ?? string.Empty,
                        Type = TypeMapper.From(propType),
                        Description = attrAttr.Description ?? string.Empty,
                        Computed = attrAttr.Computed,
                        Optional = attrAttr.Optional,
                        Required = attrAttr.Required,
                        Sensitive = attrAttr.Sensitive,
                    });
                }
            }

            return block;
        }

        private Schema.Types.NestedBlock AddNested(PropertyInfo prop)
        {
            var nestedAttr = prop.GetCustomAttribute<TFNestedAttribute>();
            if (nestedAttr == null)
                return null;

            var propType = prop.PropertyType;
            var nested = new Schema.Types.NestedBlock();
            nested.TypeName = nestedAttr.Name;
            nested.MinItems = 0;
            nested.MaxItems = 0;
            nested.Block = new Schema.Types.Block();

            if (propType.IsGenericType
                && TypeMapper.TryGetMapElementTypeFrom(propType, out var mapElementType))
            {
                nested.Nesting = Schema.Types.NestedBlock.Types.NestingMode.Map;
                AddAttrs(nested.Block, mapElementType);
            }
            else if (propType.IsGenericType
                && TypeMapper.TryGetListElementTypeFrom(propType, out var listElementType))
            {
                nested.Nesting = Schema.Types.NestedBlock.Types.NestingMode.List;
                AddAttrs(nested.Block, listElementType);
            }
            else
            {
                nested.Nesting = Schema.Types.NestedBlock.Types.NestingMode.Single;
                AddAttrs(nested.Block, propType);
            }

            return nested;
        }

        public class SchemaDetails : ISchemaDetails
        {
            public string Name { get; set; }

            public Type Type { get; set; }

            public Attribute Attribute { get; set; }

            public Schema Schema { get; set; }
        }
    }
}