using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf.Collections;
using HC.TFPlugin.Attributes;
using Microsoft.Extensions.Logging;
using Tfplugin5;

namespace HC.TFPlugin
{
    public static class SchemaHelper
    {
        private static readonly ILogger _log = LogUtil.Create(typeof(SchemaHelper));

        public static Schema GetProviderSchema(Assembly asm = null) =>
            GetPluginDetails(asm).Provider.GetSchemaFromType<TFProviderAttribute>(ta =>
                new Schema
                {
                    Version = ta.attr.Version,
                    Block = new Schema.Types.Block(),
                });

        public static IDictionary<string, Schema> GetDataSourceSchemas(Assembly asm = null) =>
            (GetPluginDetails(asm).DataSources ?? Type.EmptyTypes
                ).GetSchemasFromTypes<TFDataSourceAttribute>(ta =>
                    (ta.attr.Name, new Schema
                    {
                        Version = ta.attr.Version,
                        Block = new Schema.Types.Block(),
                    }));

        public static IDictionary<string, Schema> GetResourceSchemas(Assembly asm = null) =>
            (GetPluginDetails(asm).Resources ?? Type.EmptyTypes
                ).GetSchemasFromTypes<TFResourceAttribute>(ta =>
                    (ta.attr.Name, new Schema
                    {
                        Version = ta.attr.Version,
                        Block = new Schema.Types.Block(),
                    }));

        public static TFPluginAttribute GetPluginDetails(Assembly asm = null) =>
            (asm ?? Assembly.GetEntryAssembly()).GetCustomAttribute<TFPluginAttribute>();

        public static Schema GetSchemaFromType<TAttr>(this Type type)
            where TAttr : Attribute => GetSchemaFromType<TAttr>(type, ta =>
                new Schema
                {
                    Version = 1L,
                    Block = new Schema.Types.Block(),
                });

        public static Schema GetSchemaFromType<TAttr>(this Type type,
            Func<(Type type, TAttr attr), Schema> schemaCreator)
            where TAttr : Attribute =>
            schemaCreator((type, type.GetCustomAttribute<TAttr>()))
                .AddAttrsAndNested(type);

        public static IDictionary<string, Schema> GetSchemasFromTypes<TAttr>(this IEnumerable<Type> types,
            Func<(Type type, TAttr attr), (string name, Schema schema)> schemaCreator = null) where TAttr : Attribute => types
                .Select(x => (type: x, attr: x.GetCustomAttribute<TAttr>()))
                .Where(x => x.attr != null)
                .Select(ta =>
                    {
                        var s = schemaCreator(ta);
                        s.schema.AddAttrsAndNested(ta.type);
                        return s;
                    })
                .ToDictionary(ns => ns.name, ns => ns.schema);

        public static Schema AddAttrsAndNested(this Schema schema, Type t)
        {
            if (schema.Block == null)
                schema.Block = new Schema.Types.Block();

            schema.Block.AddAttrsAndNested(t);

            return schema;
        }

        public static Schema.Types.Block AddAttrsAndNested(this Schema.Types.Block block, Type t)
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

        public static Schema.Types.NestedBlock AddNested(PropertyInfo prop)
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
                && TypeMapper.MapElementTypeFrom(propType) is Type mapElementType)
            {
                nested.Nesting = Schema.Types.NestedBlock.Types.NestingMode.Map;
                nested.Block.AddAttrsAndNested(mapElementType);
            }
            else if (propType.IsGenericType
                && TypeMapper.ListElementTypeFrom(propType) is Type listElementType)
            {
                nested.Nesting = Schema.Types.NestedBlock.Types.NestingMode.List;
                nested.Block.AddAttrsAndNested(listElementType);
            }
            else
            {
                nested.Nesting = Schema.Types.NestedBlock.Types.NestingMode.Single;
                nested.Block.AddAttrsAndNested(propType);
            }

            return nested;
        }
    }
}