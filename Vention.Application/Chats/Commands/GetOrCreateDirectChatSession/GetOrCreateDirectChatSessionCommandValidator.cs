using FluentValidation;

namespace Vention.Application.Chats.Commands.GetOrCreateDirectChatSession
{
    public sealed class GetOrCreateDirectChatSessionCommandValidator
        : AbstractValidator<GetOrCreateDirectChatSessionCommand>
    {
        public GetOrCreateDirectChatSessionCommandValidator()
        {
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.InitiatorUserId).NotEmpty();
            RuleFor(x => x.ParticipantUserId).NotEmpty();

            RuleFor(x => x)
                .Must(x => x.InitiatorUserId != x.ParticipantUserId)
                .WithMessage("Cannot open a direct chat session with yourself.")
                .OverridePropertyName(nameof(GetOrCreateDirectChatSessionCommand.ParticipantUserId));
        }
    }
}
