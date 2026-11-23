using HowsYourDay.Server.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HowsYourDay.Server.Services;

public class DatabaseHealthCheck(HowsYourDayAppDbContext database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await database.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy();
    }
}
