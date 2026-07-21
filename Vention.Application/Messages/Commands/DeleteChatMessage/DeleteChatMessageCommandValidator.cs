using FluentValidation;

namespace Vention.Application.Messages.Commands.DeleteChatMessage
{
    public sealed class DeleteChatMessageCommandValidator : AbstractValidator<DeleteChatMessageCommand>
    {
        public DeleteChatMessageCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
