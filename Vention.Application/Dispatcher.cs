using Microsoft.Extensions.DependencyInjection;
using Vention.Application.Messaging;

namespace Vention.Application
{
    namespace Vention.Application
    {
        public sealed class Dispatcher : IDispatcher
        {
            private readonly IServiceProvider _provider;
            public Dispatcher(IServiceProvider provider) => _provider = provider;

            public Task Send(ICommand command, CancellationToken ct = default)
            {
                var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
                dynamic handler = _provider.GetRequiredService(handlerType);
                return handler.Handle((dynamic)command, ct);
            }

            public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)
            {
                var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
                dynamic handler = _provider.GetRequiredService(handlerType);
                return handler.Handle((dynamic)command, ct);
            }

            public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
            {
                var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
                dynamic handler = _provider.GetRequiredService(handlerType);
                return handler.Handle((dynamic)query, ct);
            }
        }
    }
}