using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vention.Infrastructure.Database;

namespace Vention.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<VentionDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));

            return services;
        }
    }
}
