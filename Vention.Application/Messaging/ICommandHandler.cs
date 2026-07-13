namespace Vention.Application.Messaging
{

    public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
    {
        Task<TResponse> Handle(TCommand command, CancellationToken ct);
    }


    public interface ICommandHandler<in TCommand> 
    {
        Task Handle(TCommand command, CancellationToken ct);
    }
}