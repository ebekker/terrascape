using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf.Collections;
using HC.TFPlugin.Attributes;
using Tfplugin5;

namespace HC.TFPlugin
{
    public static class SchemaHelper
    {
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
                .AddAttributes(type);

        public static IDictionary<string, Schema> GetSchemasFromTypes<TAttr>(this IEnumerable<Type> types,
            Func<(Type type, TAttr attr), (string name, Schema schema)> schemaCreator = null) where TAttr : Attribute => types
                .Select(x => (type: x, attr: x.GetCustomAttribute<TAttr>()))
                .Where(x => x.attr != null)
                .Select(ta =>
                    {
                        var s = schemaCreator(ta);
                        s.schema.AddAttributes(ta.type);
                        return s;
                    })
                .ToDictionary(ns => ns.name, ns => ns.schema);

        public static Schema AddAttributes(this Schema schema, Type t)
        {
//Dumper.Out.WriteLine($"  Adding attributes for [{t.FullName}]");
            if (schema.Block == null)
                schema.Block = new Schema.Types.Block();
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attrAttr = p.GetCustomAttribute<TFAttributeAttribute>();
                if (attrAttr == null)
                    continue;

//Dumper.Out.WriteLine($"  Adding attribute[{attrAttr.Name}]");

                schema.Block.Attributes.Add(new Schema.Types.Attribute
                {
                    Name = attrAttr.Name ?? string.Empty,
                    Type = TypeMapper.From(p.PropertyType),
                    Description = attrAttr.Description ?? string.Empty,
                    Computed = attrAttr.Computed,
                    Optional = attrAttr.Optional,
                    Required = attrAttr.Required,
                    Sensitive = attrAttr.Sensitive,
                });
            }

            return schema;
        }
    }
}