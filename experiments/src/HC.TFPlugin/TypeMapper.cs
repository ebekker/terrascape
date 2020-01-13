using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;

namespace HC.TFPlugin
{
    public class TypeMapper
    {
        // Derived from:
        //  https://github.com/hashicorp/terraform/blob/4de0b33097bd599fca83b0e9a8c7bb5987c2ceab/helper/schema/core_schema.go#L176
        //  https://github.com/hashicorp/terraform/blob/master/helper/schema/valuetype.go
        //  https://github.com/hashicorp/terraform/blob/master/helper/schema/schema.go

        public static readonly ByteString TypeDynamic = ByteString.CopyFromUtf8(@"""dynamic""");
        public static readonly ByteString TypeBool = ByteString.CopyFromUtf8(@"""bool""");
        public static readonly ByteString TypeNumber = ByteString.CopyFromUtf8(@"""number""");
        public static readonly ByteString TypeInt = ByteString.CopyFromUtf8(@"""number""");//(@"""int""");
        public static readonly ByteString TypeLong = ByteString.CopyFromUtf8(@"""number""");//(@"""long""");
        public static readonly ByteString TypeFloat = ByteString.CopyFromUtf8(@"""float64""");
        public static readonly ByteString TypeString = ByteString.CopyFromUtf8(@"""string""");

        public static ByteString TypeMap(ByteString type) =>
            ByteString.CopyFromUtf8($@"[""map"",{type.ToStringUtf8()}]");

        public static ByteString TypeList(ByteString type) =>
            ByteString.CopyFromUtf8($@"[""list"",{type.ToStringUtf8()}]");

        public static ByteString From(Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Computed<>))
                return From(t.GetGenericArguments()[0]);

            if (typeof(bool) == t)
                return TypeBool;

            if (typeof(int) == t)
                return TypeInt;

            if (typeof(long) == t)
                return TypeInt;

            // TODO: Is there a float128?
            if (typeof(float) == t || typeof(double) == t)
                return TypeFloat;
            
            if (typeof(string) == t)
                return TypeString;

            var nullableElementType = NullableElementTypeFrom(t);
            if (nullableElementType != null)
            {
                try
                {
                    return From(nullableElementType);
                }
                catch (Exception)
                {
                    throw new NotSupportedException(
                        $"failed to resolve nullable value type [{nullableElementType.FullName}]");
                }
            }

            var mapElementType = MapElementTypeFrom(t);
            if (mapElementType != null)
            {
                try
                {
                    return TypeMap(From(mapElementType));
                }
                catch (Exception ex)
                {
                    throw new NotSupportedException(
                        $"failed to resolve map value type [{mapElementType.FullName}]", ex);
                }
            }

            var listElementType = ListElementTypeFrom(t);
            if (listElementType != null)
            {
                try
                {
                    return TypeList(From(listElementType));
                }
                catch (Exception ex)
                {
                    throw new NotSupportedException("failed to resolve list value type", ex);
                }
            }

            throw new NotSupportedException("unable to map native type to Schema type");
        }

        public static Type NullableElementTypeFrom(Type type)
        {
            var nullableSubclass = GetSubclassOfGenericTypeDefinition(typeof(Nullable<>), type);
            return nullableSubclass?.GenericTypeArguments[0];                
        }

        public static Type MapElementTypeFrom(Type type)
        {
            var mapSubclass = GetSubclassOfGenericTypeDefinition(typeof(IDictionary<,>), type);
            if (mapSubclass != null && mapSubclass.GenericTypeArguments[0] != typeof(string))
                throw new NotSupportedException("maps can only support string keys");
            return mapSubclass?.GenericTypeArguments[1];
        }

        public static Type ListElementTypeFrom(Type type)
        {
            var listSubclass = GetSubclassOfGenericTypeDefinition(typeof(IList<>), type);
            return listSubclass?.GenericTypeArguments[0];
        }

        // Based on:
        //  https://stackoverflow.com/a/457708/5428506
        public static Type GetSubclassOfGenericTypeDefinition(Type genericTypeDef, Type generic) {
            if (genericTypeDef == null)
                throw new ArgumentNullException(nameof(genericTypeDef));
            if (generic == null)
                throw new ArgumentNullException(nameof(generic));

            if (!genericTypeDef.IsGenericTypeDefinition || generic.IsGenericTypeDefinition)
                return null;
                
            while (generic != null && generic != typeof(object)) {
                var cur = generic.IsGenericType ? generic.GetGenericTypeDefinition() : generic;
                if (genericTypeDef == cur) {
                    return generic;
                }
                foreach (var i in generic.GetInterfaces())
                {
                    if (i.IsGenericType && genericTypeDef == i.GetGenericTypeDefinition())
                        return i;
                }

                generic = generic.BaseType;
            }
            return null;
        }        
    }
}