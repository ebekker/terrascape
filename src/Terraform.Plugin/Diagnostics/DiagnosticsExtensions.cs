using System.Collections.Generic;
using Tfplugin5;

namespace Terraform.Plugin.Diagnostics
{
    public static class DiagnosticsExtensions
    {
        public static int Count(this TFDiagnostics diags) => diags?._diagnostics?.Count ?? 0;

        public static IEnumerable<Diagnostic> All(this TFDiagnostics diags) => diags?._diagnostics;

        public static IHasDiagnostics AddInvalid(this IHasDiagnostics hasDiags, string summary, string detail = null, TFSteps steps = null) =>
            Add(hasDiags, Diagnostic.Types.Severity.Invalid, summary, detail, steps);

        public static IHasDiagnostics  AddError(this IHasDiagnostics hasDiags, string summary, string detail = null, TFSteps steps = null) =>
            Add(hasDiags, Diagnostic.Types.Severity.Error, summary, detail, steps);

        public static IHasDiagnostics  AddWarning(this IHasDiagnostics hasDiags, string summary, string detail = null, TFSteps steps = null) =>
            Add(hasDiags, Diagnostic.Types.Severity.Warning, summary, detail, steps);

        static IHasDiagnostics  Add(this IHasDiagnostics hasDiags, Diagnostic.Types.Severity severity,
            string summary, string detail = null, TFSteps steps = null)
        {
            if (hasDiags.Diagnostics == null)
                hasDiags.Diagnostics = new TFDiagnostics();

            hasDiags.Diagnostics._diagnostics.Add(new Diagnostic
            {
                Severity = severity,
                Summary = summary ?? string.Empty,
                Detail = detail ?? string.Empty,
                Attribute = steps?.ToPath(),
            });
            return hasDiags;
        }
    }
}