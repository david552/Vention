using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Vention.Application.Authorization;
using Vention.Application.Messaging;
using Vention.Application.Users;

namespace Vention.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDispatcher, Dispatcher>();
            services.AddScoped<UserAuthorizationService>();
            services.AddScoped<OrganizationAuthorizationService>();
            services.AddScoped<ChatAuthorizationService>();
            services.AddScoped<UserResponseComposer>();
            services.AddScoped<ActiveOrganizationContextService>(); 

            var assembly = typeof(DependencyInjection).Assembly;

            services.AddValidatorsFromAssembly(assembly);

            RegisterOpenGenericImplementations(services, assembly, typeof(ICommandHandler<,>));
            RegisterOpenGenericImplementations(services, assembly, typeof(ICommandHandler<>));
            RegisterOpenGenericImplementations(services, assembly, typeof(IQueryHandler<,>));
            RegisterOpenGenericImplementations(services, assembly, typeof(IQueryHandler<>));

            TypeAdapterConfig.GlobalSettings.Scan(assembly);

            return services;
        }

        private static void RegisterOpenGenericImplementations(IServiceCollection services, Assembly assembly, Type openGenericInterface)
        {
            var matches = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface)
                    .Select(i => (Interface: i, Implementation: t)));

            foreach (var (iface, impl) in matches)
                services.AddScoped(iface, impl);
        }
    }
}
