using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vention.Application;
using Vention.Application.Abstractions;
using Vention.Presentation.Common.Services;

namespace Vention.Presentation.Common.Extensions
{

    public static class PresentationServiceExtensions
    {
        public static IServiceCollection AddPresentationGatewayAuth(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            services.AddOptions<GatewayOptions>()
                .Bind(configuration.GetSection(GatewayOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
    }
}