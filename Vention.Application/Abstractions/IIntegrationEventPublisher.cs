namespace Vention.Application.Abstractions
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;
    }
}