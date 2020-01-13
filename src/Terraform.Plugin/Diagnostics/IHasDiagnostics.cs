namespace Terraform.Plugin.Diagnostics
{
    public interface IHasDiagnostics
    {
        TFDiagnostics Diagnostics { get; set; }
    }
}