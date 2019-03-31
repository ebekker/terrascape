using System;
using System.Collections.Generic;
using System.Reflection;

namespace HC.TFPlugin.Attributes
{
    public static class TFAttributeExtensions
    {

        /// <summary>
        /// Performs a "shallow" copy of the values of each public property
        /// that's attributed with <c>TFArgumentAttribute</c>.
        /// </summary>
        public static T CopyArgumentsFrom<T>(this T target, T source)
        {
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in props)
            {
                var argAttr = p.GetCustomAttribute<TFArgumentAttribute>();
                var nestedAttr = p.GetCustomAttribute<TFNestedAttribute>();
                if (argAttr == null && nestedAttr == null)
                    continue;
                
                var val = p.GetValue(source);

                if (val == null && nestedAttr != null)
                {
                    if (TypeMapper.GetSubclassOfGenericTypeDefinition(
                        typeof(IList<>), p.PropertyType) is Type listType)
                        val = Activator.CreateInstance(
                            typeof(List<>).MakeGenericType(listType.GenericTypeArguments));
                    else if (TypeMapper.GetSubclassOfGenericTypeDefinition(
                        typeof(IDictionary<,>), p.PropertyType) is Type mapType)
                        val = Activator.CreateInstance(
                            typeof(Dictionary<,>).MakeGenericType(mapType.GenericTypeArguments));
                }

                p.SetValue(target, val);
            }

            return target;
        }
        
    }
}