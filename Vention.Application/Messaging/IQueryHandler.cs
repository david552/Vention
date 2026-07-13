namespace Vention.Application.Messaging
{

    public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
    {
        Task<TResponse> Handle(TQuery query, CancellationToken ct);
    }


    public interface IQueryHandler<in TQuery> 
    {
        Task Handle(TQuery query, CancellationToken ct);
    }
}
