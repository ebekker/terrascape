namespace HashiCorp.GoPlugin.Health
{
    /// <summary>
    /// An interface to manage health status of published gRPC services.
    /// </summary>
    /// <remarks>
    /// By implementing and only exposing this interface, we insulate clients from the
    /// implementation details of the gRPC service that implements the <see
    /// cref="https://github.com/grpc/grpc/blob/master/doc/health-checking.md"
    /// >Health Checking Service</see>.
    /// </remarks>
    public interface IHealthService
    {
        void ClearAll();
        void ClearStatus(string service);
        void SetStatus(string service, HealthStatus status);
    }
}