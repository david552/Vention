using FluentValidation;

namespace Vention.Application.Chats.Commands.RenameChatSession
{
    public sealed class RenameChatSessionCommandValidator : AbstractValidator<RenameChatSessionCommand>
    {
        public RenameChatSessionCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        }
    }
}
