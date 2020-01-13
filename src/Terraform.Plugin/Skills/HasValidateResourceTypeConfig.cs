using System;
using Terraform.Plugin.Diagnostics;

namespace Terraform.Plugin.Skills
{
    public static class HasValidateResourceTypeConfig
    {
        public interface Skill<TResource>
        {
            Result<TResource> ValidateConfig(Input<TResource> input);
        }        

        public class Input<TResource>
        {
            public TResource Config { get; set; }
        }

        public class Result<TResource> : IHasDiagnostics
        {
            public TFDiagnostics Diagnostics { get; set; }
        }

        public static bool HasValidateResourceTypeConfigSkill(this Type targetType, Type resType) =>
            targetType.HasSkill(typeof(Skill<>), resType);

        public static void InvokeValidateResourceTypeConfigSkill(
            this Type targetType,
            object target, 
            Type resType,
            Action<Type, object> writeInput = null,
            Action<Type, object> readResult = null)
        {
            var methodArgs = new[] { typeof(Input<>).MakeGenericType(resType) };
            var method = targetType.GetMethod(nameof(Skill<object>.ValidateConfig), methodArgs);
            if (method == null)
                throw new InvalidOperationException("target type does not implement skill");

            SkillsExtensions.InvokeSkill(
                target: target,
                skillType: typeof(Skill<>),
                inputType: typeof(Input<>),
                resultType: typeof(Result<>),
                genTypeArg: resType,
                skillInvokeMethod: method,
                writeInput: writeInput,
                readResult: readResult);
        }
    }
}