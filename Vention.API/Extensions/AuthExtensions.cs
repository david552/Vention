using Microsoft.OpenApi.Models;
using Vention.Application;
using Vention.Application.Abstractions;
using Vention.Application.Options;
using Vention.Presentation.Common.Services;

namespace Vention.API.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT is validated by ApiGateway. Call the gateway on port 5258"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        public static IServiceCollection AddJwtSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtSettingsOptions>()
                .Bind(configuration.GetSection("Jwt"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }
        public static IServiceCollection AddCurrentUserAccess(this IServiceCollection services, IConfiguration configuration)
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
