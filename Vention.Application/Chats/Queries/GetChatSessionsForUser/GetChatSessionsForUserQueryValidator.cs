using FluentValidation;

namespace Vention.Application.Chats.Queries.GetSessionsForUser
{
    public sealed class GetChatSessionsForUserQueryValidator : AbstractValidator<GetChatSessionsForUserQuery>
    {
        public GetChatSessionsForUserQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
