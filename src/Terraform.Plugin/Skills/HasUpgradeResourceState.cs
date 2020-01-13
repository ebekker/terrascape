using System;
using Terraform.Plugin.Diagnostics;

namespace Terraform.Plugin.Skills
{
    public static class HasUpgradeResourceState
    {
        public interface Skill<TResource>
        {
            Result<TResource> UpgradeState(Input<TResource> input);
        }

        public class Input<TResource>
        {
            /// <summary>
            /// The schema_version number recorded in the state file.
            /// </summary>
            public long Version { get; set; }
            /// <summary>
            /// JSON-encoded raw state as stored for the resource.
            /// It's the  provider's responsibility to interpret this
            /// value using the appropriate older schema.
            /// </summary>
            public TFRawState RawState { get; set; }
        }

        public class Result<TResource> : IHasDiagnostics
        {
            public TFDiagnostics Diagnostics { get; set; }
            public TResource UpgradedState { get; set; }
        }

        public static bool HasUpgradeResourceStateSkill(this Type targetType, Type configType) =>
            targetType.HasSkill(typeof(Skill<>), configType);

        public static void InvokeUpgradeResourceStateSkill(
            this Type targetType,
            object target,
            Type configType,
            Action<Type, object> writeInput = null,
            Action<Type, object> readResult = null)
        {
            var methodArgs = new[] { typeof(Input<>).MakeGenericType(configType) };
            var method = targetType.GetMethod(nameof(Skill<object>.UpgradeState), methodArgs);
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