using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Vention.Application.Messaging;

namespace Vention.Application
{
    public sealed class Dispatcher : IDispatcher
    {
        private readonly IServiceProvider _provider;

        public Dispatcher(IServiceProvider provider) => _provider = provider;

        public async Task Send(ICommand command, CancellationToken ct = default)
        {
            await ValidateAsync(command, ct);

            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
            dynamic handler = _provider.GetRequiredService(handlerType);
            await handler.Handle((dynamic)command, ct);
        }

        public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)
        {
            await ValidateAsync(command, ct);

            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
            dynamic handler = _provider.GetRequiredService(handlerType);
            return await handler.Handle((dynamic)command, ct);
        }

        public async Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
        {
            await ValidateAsync(query, ct);

            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
            dynamic handler = _provider.GetRequiredService(handlerType);
            return await handler.Handle((dynamic)query, ct);
        }

        private async Task ValidateAsync(object request, CancellationToken ct)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(request.GetType());
            if (_provider.GetService(validatorType) is not IValidator validator)
                return;

            var context = new ValidationContext<object>(request);
            var result = await validator.ValidateAsync(context, ct);

            if (!result.IsValid)
                throw new Exceptions.ValidationException(result.Errors);
        }
    }
}
