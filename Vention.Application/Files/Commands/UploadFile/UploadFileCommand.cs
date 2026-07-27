using Vention.Application.Files.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Files.Commands.UploadFile
{
    public sealed record UploadFileCommand(
        Stream Content,
        string Filename,
        string ContentType,
        long? Size,
        Guid OrganizationId,
        Guid ActingUserId) : ICommand<FileResponse>;
}
