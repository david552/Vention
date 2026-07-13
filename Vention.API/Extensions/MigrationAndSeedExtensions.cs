using Microsoft.EntityFrameworkCore;
using Vention.Infrastructure.Persistence;
using Vention.Infrastructure.Seed;

namespace Vention.API.Extensions
{
    public static class MigrationAndSeedExtensions
    {
        public static async Task SeedDatabaseAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<VentionDbContext>();

                await context.Database.MigrateAsync();

                await DataSeeder.SeedAsync(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<IHost>>();
                logger.LogError(ex, "An error occurred while migrating or seeding the database.");

               
                throw;
            }
        }
    }
}
