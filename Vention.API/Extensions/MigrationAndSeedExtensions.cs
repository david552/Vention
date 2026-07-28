using Microsoft.EntityFrameworkCore;
using Vention.Application.Abstractions;
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
                var passwordHasher = services.GetRequiredService<IPasswordHasher>();

                await context.Database.MigrateAsync();

                await DataSeeder.SeedAsync(context, passwordHasher);
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
