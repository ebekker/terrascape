using System;
using Terraform.Plugin.Diagnostics;

namespace Terraform.Plugin.Skills
{
    /// <summary>
    /// Allows the provider to validate the configuration
	/// values, and set or override any values with defaults.
    /// </summary>
    public static class HasPrepareProviderConfig
    {
        public interface Skill<TConfig>
        {
            Result<TConfig> PrepareConfig(Input<TConfig> input);            
        }

        public class Input<TConfig>
        {
            public TConfig Config { get; set; }
        }

        public class Result<TConfig> : IHasDiagnostics
        {
            public TConfig PreparedConfig { get; set; }

            public TFDiagnostics Diagnostics { get; set; }
        }

        public static bool HasPrepareProviderConfigSkill(this Type targetType, Type configType) =>
            targetType.HasSkill(typeof(Skill<>), configType);

        public static void InvokePrepareProviderConfigSkill(
            this Type targetType,
            object target, 
            Type configType,
            Action<Type, object> writeInput = null,
            Action<Type, object> readResult = null)
        {
            var methodArgs = new[] { typeof(Input<>).MakeGenericType(configType) };
            var method = targetType.GetMethod(nameof(Skill<object>.PrepareConfig), methodArgs);
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