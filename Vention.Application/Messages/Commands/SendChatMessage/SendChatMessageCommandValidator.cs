using FluentValidation;

namespace Vention.Application.Messages.Commands.SendChatMessage
{
    public sealed class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
    {
        public SendChatMessageCommandValidator()
        {
            RuleFor(x => x.ChatSessionId).NotEmpty();
            RuleFor(x => x.SenderId).NotEmpty();
            RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
        }
    }
}
