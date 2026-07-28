using FluentValidation;

namespace Vention.Application.Chats.Commands.CreateChatSession
{
    public sealed class CreateChatSessionCommandValidator : AbstractValidator<CreateChatSessionCommand>
    {
        public CreateChatSessionCommandValidator()
        {
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.InitiatorUserId).NotEmpty();
            RuleFor(x => x.ParticipantUserId).NotEmpty();

            RuleFor(x => x)
                .Must(x => x.InitiatorUserId != x.ParticipantUserId)
                .WithMessage("Cannot create a direct chat session with yourself.")
                .OverridePropertyName(nameof(CreateChatSessionCommand.ParticipantUserId));
        }
    }
}
