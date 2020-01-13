namespace Terraform.Plugin
{
    public class TFImportedResource<TResource>
    {
        public TResource State { get; set; }
        public byte[] Private { get; set; }
    }
}