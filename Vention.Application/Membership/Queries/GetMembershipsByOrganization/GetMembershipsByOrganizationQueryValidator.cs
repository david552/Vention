using FluentValidation;

namespace Vention.Application.Membership.Queries.GetMembershipsByOrganization
{
    public sealed class GetMembershipsByOrganizationQueryValidator
        : AbstractValidator<GetMembershipsByOrganizationQuery>
    {
        public GetMembershipsByOrganizationQueryValidator()
        {
            RuleFor(x => x.OrganizationId).NotEmpty();
        }
    }
}
