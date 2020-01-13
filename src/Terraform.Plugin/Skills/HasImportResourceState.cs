using System;
using System.Collections.Generic;
using Terraform.Plugin.Diagnostics;

namespace Terraform.Plugin.Skills
{
    public static class HasImportResourceState
    {
        public interface Skill<TResource>
        {
            Result<TResource> ImportResource(Input<TResource> input);
        }

        public class Input<TResource>
        {
            public string Id { get; set; }
        }

        public class Result<TResource> : IHasDiagnostics
        {
            public TFDiagnostics Diagnostics { get; set; }

            public IEnumerable<TFImportedResource<TResource>> ImportedResources { get; set; }
        }

        public static bool HasImportResourceStateSkill(this Type targetType, Type configType) =>
            targetType.HasSkill(typeof(Skill<>), configType);

        public static void InvokeImportResourceStateSkill(
            this Type targetType,
            object target, 
            Type configType,
            Action<Type, object> writeInput = null,
            Action<Type, object> readResult = null)
        {
            var methodArgs = new[] { typeof(Input<>).MakeGenericType(configType) };
            var method = targetType.GetMethod(nameof(Skill<object>.ImportResource), methodArgs);
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