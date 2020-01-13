using Terraform.Plugin.Skills;

namespace Terraform.Plugin
{
    public interface IDataSourceProvider<TDataSource> :
        HasValidateDataSourceConfig.Skill<TDataSource>,
        HasReadDataSource.Skill<TDataSource>
    { }
}
