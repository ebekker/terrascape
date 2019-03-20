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
                var attr = p.GetCustomAttribute<TFArgumentAttribute>();
                if (attr == null)
                    continue;
                p.SetValue(target, p.GetValue(source));
            }

            return target;
        }
        
    }
}