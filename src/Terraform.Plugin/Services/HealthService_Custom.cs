using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Health.V1;
using Microsoft.Extensions.Logging;
using Terraform.Plugin.Health;

namespace Terraform.Plugin.Services
{
    public partial class HealthService : IHealthService
    {
        private readonly ILogger _log;

        public HealthService(ILogger<HealthService> logger)
        {
            _log = logger;
            _log.LogInformation("HealthService constructed");
        }

        public void SetStatus(string service, HealthStatus status)
        {
            var servingStatus = status switch
            {
                HealthStatus.Serving    => HealthCheckResponse.Types.ServingStatus.Serving,
                HealthStatus.NotServing => HealthCheckResponse.Types.ServingStatus.NotServing,
                HealthStatus.Unknown    => HealthCheckResponse.Types.ServingStatus.Unknown,
                _                       => HealthCheckResponse.Types.ServingStatus.Unknown,
            };

            SetStatus(service, servingStatus);
        }
    }
}
