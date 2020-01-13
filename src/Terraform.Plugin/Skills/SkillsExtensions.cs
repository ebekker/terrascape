using System;
using System.Reflection;

namespace Terraform.Plugin.Skills
{
    public static class SkillsExtensions
    {
        public static bool HasSkill(
            this Type targetType,
            Type skillType,
            Type genTypeArg)
        {
            if (genTypeArg != null)
                skillType = skillType.MakeGenericType(genTypeArg);
            return skillType.IsAssignableFrom(targetType);
        }

        public static void InvokeSkill(
            this object target,
            Type skillType,
            Type inputType,
            Type resultType,
            MethodInfo skillInvokeMethod,
            Action<Type, object> writeInput = null,
            Action<Type, object> readResult = null)
        {
            if (skillType == null)
                throw new ArgumentNullException(nameof(skillType));
            if (inputType == null)
                throw new ArgumentNullException(nameof(inputType));
            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));
            if (skillInvokeMethod == null)
                throw new ArgumentNullException(nameof(skillInvokeMethod));


            // Construct and populate the input type instance from the request
            var input = Activator.CreateInstance(inputType);
            writeInput.Invoke(inputType, input);

            // Invoke the functional method
            var result = skillInvokeMethod.Invoke(target, new[] { input });
            if (result == null)
                throw new Exception("invocation result returned null");

            if (!resultType.IsAssignableFrom(resultType))
                throw new Exception("invocation result not of expected type or subclass");

            // Deconstruct the result and copy to response type
            readResult.Invoke(resultType, result);
        }

        public static void InvokeSkill(
            this object target,
            Type skillType,
            Type inputType,
            Type resultType,
            Type genTypeArg,
            MethodInfo skillInvokeMethod = null,
            string skillInvokeMethodName = null,
            Action<Type, object> writeInput = null,
            Action<Type, object> readResult = null)
        {
            if (skillType == null)
                throw new ArgumentNullException(nameof(skillType));
            if (inputType == null)
                throw new ArgumentNullException(nameof(inputType));
            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));
            if (skillInvokeMethod == null)
                throw new ArgumentNullException(nameof(skillInvokeMethod));

            if (genTypeArg != null)
            {
                skillType = skillType.MakeGenericType(genTypeArg);
                inputType = inputType.MakeGenericType(genTypeArg);
                resultType = resultType.MakeGenericType(genTypeArg);
            }

            // Construct and populate the input type instance from the request
            var input = Activator.CreateInstance(inputType);
            writeInput?.Invoke(inputType, input);

            // Invoke the functional method
            var result = skillInvokeMethod.Invoke(target, new[] { input });
            if (result == null)
                throw new Exception("invocation result returned null");

            if (!resultType.IsAssignableFrom(resultType))
                throw new Exception("invocation result not of expected type or subclass");

            // Deconstruct the result and copy to response type
            readResult.Invoke(resultType, result);
        }
    }
}