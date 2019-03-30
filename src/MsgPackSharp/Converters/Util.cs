using System;

namespace MsgPackSharp.Converters
{
    public class Util
    {
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