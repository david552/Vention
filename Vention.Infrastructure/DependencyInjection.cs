using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

using Vention.Application.Abstractions;
using Vention.Application.Abstractions.Auth;
using Vention.Application.Abstractions.DocumentChunks;
using Vention.Application.Abstractions.Files;
using Vention.Application.Abstractions.Llm;
using Vention.Application.Abstractions.Messaging;
using Vention.Application.Options;
using Vention.Domain.Chats;
using Vention.Domain.DocumentChunks;
using Vention.Domain.Files;
using Vention.Domain.Membership;
using Vention.Domain.Messages;
using Vention.Domain.Organizations;
using Vention.Domain.Users;
using Vention.Infrastructure.Messaging;
using Vention.Infrastructure.Persistence;
using Vention.Infrastructure.Persistence.Repositories;
using Vention.Infrastructure.Services;
using Vention.Infrastructure.Services.DocumentExtraction;
using Vention.Infrastructure.Services.DocumentExtraction.Strategies;
using Vention.Infrastructure.Services.Ollama;

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

            services.AddOptions<OllamaOptions>()
                .Bind(configuration.GetSection(OllamaOptions.SectionName))
                .ValidateDataAnnotations()
                .Validate(
                    o => o.EmbeddingDimensions == OllamaOptions.DefaultEmbeddingDimensions,
                    "Ollama:EmbeddingDimensions must match the DB vector column. " +
                    "If you change the model/dims, add a new EF migration.")
                .ValidateOnStart();

            services.AddSingleton<IOllamaRequestGate, OllamaRequestGate>();


            services.AddOptions<RagOptions>()
                .Bind(configuration.GetSection(RagOptions.SectionName))
                .ValidateDataAnnotations()
                .Validate(o => o.ChunkOverlap < o.ChunkSize, "Rag:ChunkOverlap must be less than Rag:ChunkSize.")
                .ValidateOnStart();


            services.AddDbContext<VentionDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PostgresConnection"),
                o => o.UseVector())
                  .EnableSensitiveDataLogging()
                  .LogTo(Console.WriteLine, LogLevel.Warning));

            services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

            services.AddHttpClient<ILlmService, OllamaLlmService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

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
            services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
            services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

            services.AddSingleton<IExtractionStrategy, TxtExtractionStrategy>();
            services.AddSingleton<IExtractionStrategy, PdfExtractionStrategy>();
            services.AddSingleton<IExtractionStrategy, WordExtractionStrategy>();
            services.AddSingleton<IDocumentExtractor, DocumentExtractorService>();
            services.AddSingleton<ITextChunker, TextChunkerService>();


            services.AddMassTransitMessaging(massTransitHost, consumerAssembly);

            var redisConnection = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Redis connection string is not configured.");

            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
            services.AddSingleton<IPresenceTracker, RedisPresenceTracker>();


            return services;
        }
    }
}
