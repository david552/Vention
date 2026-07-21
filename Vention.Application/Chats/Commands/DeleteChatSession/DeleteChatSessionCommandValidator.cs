using FluentValidation;

namespace Vention.Application.Chats.Commands.DeleteChatSession
{
    public sealed class DeleteChatSessionCommandValidator : AbstractValidator<DeleteChatSessionCommand>
    {
        public DeleteChatSessionCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
