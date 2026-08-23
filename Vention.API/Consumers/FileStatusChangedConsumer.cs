using MassTransit;
using Vention.Application.Abstractions;
using Vention.Application.Files.IntegrationEvents;
using Vention.Domain.Files;

namespace Vention.API.Consumers
{

    public sealed class FileStatusChangedConsumer : IConsumer<FileStatusChanged>
    {
        private readonly INotificationPublisher _notificationPublisher;
        private readonly ILogger<FileStatusChangedConsumer> _logger;

        public FileStatusChangedConsumer(
            INotificationPublisher notificationPublisher,
            ILogger<FileStatusChangedConsumer> logger)
        {
            _notificationPublisher = notificationPublisher;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<FileStatusChanged> context)
        {
            var message = context.Message;
            var ct = context.CancellationToken;

            _logger.LogInformation(
                "FileStatusChanged received. FileId={FileId}, Status={Status}, OwnerId={OwnerId}",
                message.FileId, message.Status, message.OwnerId);

            switch (message.Status)
            {
                case FileStatus.Processing:
                    await _notificationPublisher.NotifyJobStartedAsync(
                        message.OrganizationId, message.FileId, message.Filename, ct);
                    break;

                case FileStatus.Processed:
                case FileStatus.Error:
                    await _notificationPublisher.NotifyJobFinishedAsync(
                        message.OrganizationId, message.FileId, message.Filename, ct);
                    break;
            }
        }
    }
}