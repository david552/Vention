namespace Vention.Application.Messaging
{

    public interface IDispatcher
    {
        Task Send(ICommand command, CancellationToken ct = default);
        Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default);
        Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default);
    }
}