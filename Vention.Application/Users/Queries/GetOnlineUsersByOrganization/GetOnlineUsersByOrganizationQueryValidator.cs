using FluentValidation;

namespace Vention.Application.Users.Queries.GetOnlineUsersByOrganization
{
    public sealed class GetOnlineUsersByOrganizationQueryValidator
        : AbstractValidator<GetOnlineUsersByOrganizationQuery>
    {
        public GetOnlineUsersByOrganizationQueryValidator()
        {
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.ActingUserId).NotEmpty();
        }
    }
}
