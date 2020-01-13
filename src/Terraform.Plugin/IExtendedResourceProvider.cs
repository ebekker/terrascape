using Terraform.Plugin.Skills;

namespace Terraform.Plugin
{
    public interface IExtendedResourceProvider<TResource> :
        IResourceProvider<TResource>,
        HasUpgradeResourceState.Skill<TResource>,
        HasImportResourceState.Skill<TResource>
    { }
}
