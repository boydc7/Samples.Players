using Microsoft.Extensions.Diagnostics.HealthChecks;
using Samples.Players.Configuration;

namespace Samples.Players;

internal class ServicePingHealthCheck : IHealthCheck
{
    private ServicePingHealthCheck() { }

    public static ServicePingHealthCheck Instance { get; } = new();

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
        => Task.FromResult(SampleShutdownCancellationSource.Instance.Token.IsCancellationRequested
                               ? HealthCheckResult.Unhealthy("InShutdown", new ApplicationException("In Shutdown"))
                               : HealthCheckResult.Healthy("OK"));
}
