using Vention.Application.Messaging;
using Vention.Application.Rag.Contracts;

namespace Vention.Application.Rag.Commands.AskQuestion
{
    public sealed record AskQuestionCommand(
        Guid OrganizationId,
        Guid ActingUserId,
        string Question,
        Guid? FileId) : ICommand<AskQuestionResponse>;
}