using FluentValidation;

namespace Vention.Application.Membership.Queries.GetMembershipById
{
    public sealed class GetMembershipByIdQueryValidator : AbstractValidator<GetMembershipByIdQuery>
    {
        public GetMembershipByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
