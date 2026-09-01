namespace Vention.Application.Abstractions
{

    public interface INotificationClient
    {
        Task JobStarted(FileJobNotification notification);
        Task JobFinished(FileJobNotification notification);
    }
    public sealed record FileJobNotification(Guid FileId, string FileName);
}
