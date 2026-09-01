using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using Vention.API.Hubs;
using Vention.API.Services;
using Vention.Application.Abstractions;
using Vention.Application.Options;

namespace Vention.API.Extensions
{

    public static class SignalRExtensions
    {
        public static IServiceCollection AddVentionSignalR(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var redisConnection = configuration.GetConnectionString("redis")
                ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

            var channelPrefix = configuration
                .GetSection(SignalRRedisOptions.SectionName)
                .Get<SignalRRedisOptions>()?.ChannelPrefix;


            services.AddSignalR(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            })
            .AddStackExchangeRedis(redisConnection, options =>
            {
                if (!string.IsNullOrWhiteSpace(channelPrefix))
                {
                    options.Configuration.ChannelPrefix = RedisChannel.Literal(channelPrefix);
                }
            });

            services.AddScoped<INotificationPublisher, SignalRNotificationPublisher>();

            return services;
        }

        public static WebApplication MapVentionHubs(this WebApplication app)
        {
            app.MapHub<NotificationHub>(NotificationHub.Route);

            return app;
        }
    }
}