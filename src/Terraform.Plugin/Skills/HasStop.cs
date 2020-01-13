using System;

namespace Terraform.Plugin.Skills
{
    /// <summary>
    /// Allows the provider to be notified when
    /// the plugin has been requested to stop.
    /// </summary>
    public static class HasStop
    {
        public interface Skill
        {
            Result Stop(Input input);
        }

        public class Input
        {
            // Empty input but preserves the conventions of Skill-supporting types
        }

        public class Result
        {
            public string Error { get; set; }
        }

        public static bool HasStopSkill(this Type targetType) => targetType is Skill;

        public static void InvokeStopSkill(
            this Type targetType,
            object target,
            Action<Type, object> writeInput = null,
            Action<Type, object> readResult = null)
        {
            var methodArgs = new[] { typeof(Input) };
            var method = targetType.GetMethod(nameof(Skill.Stop), methodArgs);
            if (method == null)
                throw new InvalidOperationException("target type does not implement skill");

            SkillsExtensions.InvokeSkill(
                target: target,
                skillType: typeof(Skill),
                inputType: typeof(Input),
                resultType: typeof(Result),
                skillInvokeMethod: method,
                writeInput: writeInput,
                readResult: readResult);
        }
    }
}