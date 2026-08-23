namespace Vention.Application.Abstractions
{

    public interface INotificationPublisher
    {
        Task NotifyJobStartedAsync(
            Guid organizationId,
            Guid fileId,
            string fileName,
            CancellationToken cancellationToken = default);

        Task NotifyJobFinishedAsync(
            Guid organizationId,
            Guid fileId,
            string fileName,
            CancellationToken cancellationToken = default);
    }
}