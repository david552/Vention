using Vention.Application.Messaging;

namespace Vention.Application.Files.Commands.DeleteFile
{
    public sealed record DeleteFileCommand(
        Guid FileId,
        Guid OrganizationId,
        Guid ActingUserId) : ICommand;
}