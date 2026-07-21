using FluentValidation;

namespace Vention.Application.Chats.Queries.GetChatSessionMembers
{
    public sealed class GetChatSessionMembersQueryValidator : AbstractValidator<GetChatSessionMembersQuery>
    {
        public GetChatSessionMembersQueryValidator()
        {
            RuleFor(x => x.SessionId).NotEmpty();
        }
    }
}
