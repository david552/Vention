using FluentValidation;

namespace Vention.Application.Membership.Queries.GetMembershipsByUser
{
    public sealed class GetMembershipsByUserQueryValidator : AbstractValidator<GetMembershipsByUserQuery>
    {
        public GetMembershipsByUserQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
