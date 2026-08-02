using Vention.Application.Files.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Files.Commands.ProcessFile
{
    public sealed record ProcessFileCommand(
        Guid FileId,
        Guid OrganizationId,
        Guid ActingUserId) : ICommand<FileResponse>;
}