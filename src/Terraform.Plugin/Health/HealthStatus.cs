namespace Terraform.Plugin.Health
{
    public enum HealthStatus
    {
        // These values map directly to their corresponding enum
        // values defined in the gRPC proto -- DO NOT CHANGE!

        Unknown = 0,
        Serving = 1,
        NotServing = 2,        
    }
}