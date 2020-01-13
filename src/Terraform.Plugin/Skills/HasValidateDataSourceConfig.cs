using System;
using System.Reflection;
using Terraform.Plugin.Diagnostics;

namespace Terraform.Plugin.Skills
{
    /// <summary>
    /// Allows the provider to validate the Data Source
    /// configuration before invoking subsequent operations with it.
    /// </summary>
    public static class HasValidateDataSourceConfig
    {
        public interface Skill<TDataSource>
        {
            Result<TDataSource> ValidateConfig(Input<TDataSource> input);
        }

        public class Input<TDataSource>
        {
            public TDataSource Config { get; set; }
        }

        public class Result<TDataSource> : IHasDiagnostics
        {
            public TFDiagnostics Diagnostics { get; set; }
        }

        public static bool HasValidateDataSourceConfigSkill(this Type targetType, Type resType) =>
            targetType.HasSkill(typeof(Skill<>), resType);

        public static void InvokeValidateDataSourceConfigSkill(
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