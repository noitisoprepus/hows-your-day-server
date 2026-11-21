using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HowsYourDay.Server.Data;

public class HowsYourDayAppDbContextFactory : IDesignTimeDbContextFactory<HowsYourDayAppDbContext>
{
    public HowsYourDayAppDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets<HowsYourDayAppDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured. Set it with user-secrets or an environment variable before running EF Core commands.");
        }

        var options = new DbContextOptionsBuilder<HowsYourDayAppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new HowsYourDayAppDbContext(options);
    }
}
