using FluentValidation;

namespace Vention.Application.Chats.Queries.GetSessionsForUser
{
    public sealed class GetSessionsForUserQueryValidator : AbstractValidator<GetSessionsForUserQuery>
    {
        public GetSessionsForUserQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
