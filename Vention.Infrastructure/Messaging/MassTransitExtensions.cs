using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;
using Vention.Application.Exceptions;
using Vention.Application.Options;
using Vention.Infrastructure.Persistence;

namespace Vention.Infrastructure.Messaging
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddMassTransitMessaging(
            this IServiceCollection services,
            MassTransitHostKind hostKind,
            Assembly? consumerAssembly = null)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                if (hostKind == MassTransitHostKind.Worker)
                {
                    if (consumerAssembly is null)
                        throw new ArgumentException(
                            "Worker host requires a consumer assembly.",
                            nameof(consumerAssembly));

                    x.AddConsumers(consumerAssembly);

                    x.AddConfigureEndpointsCallback((context, name, cfg) =>
                    {
                        cfg.UseMessageRetry(r =>
                        {
                            r.Intervals(
                                TimeSpan.FromMilliseconds(100),
                                TimeSpan.FromMilliseconds(500),
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(5));

                            r.Ignore<ArgumentException>();                 
                            r.Ignore<NotFoundException>();               
                            r.Ignore<ValidationException>();
                        });

                        cfg.UseEntityFrameworkOutbox<VentionDbContext>(context);
                    });
                }

                x.AddEntityFrameworkOutbox<VentionDbContext>(o =>
                {
                    o.QueryDelay = TimeSpan.FromSeconds(1);
                    o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                    o.UsePostgres();

                    if (hostKind == MassTransitHostKind.Api)
                        o.UseBusOutbox();
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMq = context.GetRequiredService<IOptions<RabbitMqSettingsOptions>>().Value;

                    cfg.Host(rabbitMq.Host, rabbitMq.Port, rabbitMq.VirtualHost, h =>
                    {
                        h.Username(rabbitMq.Username);
                        h.Password(rabbitMq.Password);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}