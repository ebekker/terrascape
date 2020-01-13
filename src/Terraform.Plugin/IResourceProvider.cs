using Terraform.Plugin.Skills;

namespace Terraform.Plugin
{
    public interface IResourceProvider<TResource> :
        HasValidateResourceTypeConfig.Skill<TResource>,
        HasPlanResourceChange.Skill<TResource>,
        HasApplyResourceChange.Skill<TResource>,
        HasUpgradeResourceState.Skill<TResource>,
        HasReadResource.Skill<TResource>
    { }
}