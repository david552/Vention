using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Reflection;
using Vention.Application.Abstractions;
using Vention.Domain.Chats;
using Vention.Domain.Files;
using Vention.Domain.Membership;
using Vention.Domain.Messages;
using Vention.Domain.Organizations;
using Vention.Domain.Users;
using Vention.Infrastructure.Messaging;
using Vention.Infrastructure.Persistence;
using Vention.Infrastructure.Persistence.Repositories;
using Vention.Infrastructure.Services;

namespace Vention.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            MassTransitHostKind massTransitHost = MassTransitHostKind.Api,
            Assembly? consumerAssembly = null)
        {
            services.AddDbContext<VentionDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PostgresConnection"))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Warning));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IMembershipRepository, MembershipRepository>();
            services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
            services.AddScoped<IChatSessionMemberRepository, ChatSessionMemberRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IFileStorageService, FileSystemFileStorageService>();
            services.AddScoped<IStoredFileRepository, StoredFileRepository>();
            services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

            services.AddMassTransitMessaging(massTransitHost, consumerAssembly);

            var redisConnection = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Redis connection string is not configured.");

            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
            services.AddSingleton<IPresenceTracker, RedisPresenceTracker>();


            return services;
        }
    }
}
