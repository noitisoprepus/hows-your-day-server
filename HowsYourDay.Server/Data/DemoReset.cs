using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HowsYourDay.Server.Data;

internal static class DemoReset
{
    internal static async Task<int> RunAsync(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.Error.WriteLine("ConnectionStrings:DefaultConnection is required for the demo reset.");
            return 1;
        }

        try
        {
            var database = new NpgsqlConnectionStringBuilder(connectionString)
            {
                IncludeErrorDetail = false,
                LogParameters = false
            };
            var options = new DbContextOptionsBuilder<HowsYourDayAppDbContext>()
                .UseNpgsql(database.ConnectionString)
                .Options;
            await using var context = new HowsYourDayAppDbContext(options);
            var deleted = await context.Users.ExecuteDeleteAsync();
            Console.WriteLine($"Demo reset removed {deleted} accounts.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Demo reset failed ({exception.GetType().Name}).");
            return 1;
        }
    }
}
