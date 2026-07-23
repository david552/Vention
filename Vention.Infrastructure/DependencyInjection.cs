using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vention.Application.Abstractions;
using Vention.Domain.Chats;
using Vention.Domain.Membership;
using Vention.Domain.Messages;
using Vention.Domain.Organizations;
using Vention.Domain.Users;
using Vention.Infrastructure.Persistence;
using Vention.Infrastructure.Persistence.Repositories;

namespace Vention.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<VentionDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PostgresConnection"))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IMembershipRepository, MembershipRepository>();
            services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
            services.AddScoped<IChatSessionMemberRepository, ChatSessionMemberRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

            return services;
        }
    }
}
