using System;
using Terraform.Plugin.Diagnostics;

namespace Terraform.Plugin.Skills
{
    public static class HasApplyResourceChange
    {
        public interface Skill<TResource>
        {
            Result<TResource> ApplyChange(Input<TResource> input);
        }

        public class Input<TResource>
        {
            public TFResourceChangeType ChangeType { get; set; }
            public TResource PriorState { get; set; }
            public TResource Config { get; set; }
            public byte[] PlannedPrivate { get; set; }
            public TResource PlannedState { get; set; }
        }

        public class Result<TResource> : IHasDiagnostics
        {
            public TFDiagnostics Diagnostics { get; set; }
            public TResource NewState { get; set; }
            public byte[] Private { get; set; }
        }

        public static bool HasApplyResourceChangeSkill(this Type targetType, Type configType) =>
            targetType.HasSkill(typeof(Skill<>), configType);

        public static void InvokeApplyResourceChangeSkill(
            this Type targetType,
            object target, 
            Type configType,
            Action<Type, object> writeInput = null,
            Action<Type, object> readResult = null)
        {
            var methodArgs = new[] { typeof(Input<>).MakeGenericType(configType) };
            var method = targetType.GetMethod(nameof(Skill<object>.ApplyChange), methodArgs);
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