namespace Terraform.Plugin
{
    /// <summary>
    /// Defines behavior for a computed value returned from a resource or data instance.
    /// </summary>
    public interface IComputed
    {
        bool IsKnown { get; }

        object GetValue();
    }
}