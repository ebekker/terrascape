using System;
using Terraform.Plugin.Diagnostics;

namespace Terraform.Plugin.Skills
{
    /// <summary>
    /// Allows setting the configuration on the provider to be
    /// used with subsequent Data Source and Resource operations.
    /// </summary>
    public static class HasConfigure
    {
        public interface Skill<TConfig>
        {
            Result<TConfig> Configure(Input<TConfig> input);
        }

        public class Input<TConfig>
        {
            public string TerraformVersion { get; set; }

            public TConfig Config { get; set; }
        }

        public class Result<TConfig> : IHasDiagnostics
        {
            public TFDiagnostics Diagnostics { get; set; }
        }

        public static bool HasConfigureSkill(this Type targetType, Type configType) =>
            targetType.HasSkill(typeof(Skill<>), configType);

        public static void InvokeConfigureSkill(
            this Type targetType,
            object target, 
            Type configType,
            Action<Type, object> writeInput = null,
            Action<Type, object> readResult = null)
        {
            var methodArgs = new[] { typeof(Input<>).MakeGenericType(configType) };
            var method = targetType.GetMethod(nameof(Skill<object>.Configure), methodArgs);
            if (method == null)
                throw new InvalidOperationException("target type does not implement skill");

            SkillsExtensions.InvokeSkill(
                target: target,
                skillType: typeof(Skill<>),
                inputType: typeof(Input<>),
                resultType: typeof(Result<>),
                genTypeArg: configType,
                skillInvokeMethod: method,
                writeInput: writeInput,
                readResult: readResult);
        }
    }
}