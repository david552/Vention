using FluentValidation;

namespace Vention.Application.Membership.Queries.GetMembershipsByUserIds
{

    public sealed class GetMembershipsByUserIdsQueryValidator
        : AbstractValidator<GetMembershipsByUserIdsQuery>
    {
        public GetMembershipsByUserIdsQueryValidator()
        {
            RuleFor(x => x.ActingUserId).NotEmpty();
            RuleFor(x => x.UserIds).NotNull();
            RuleForEach(x => x.UserIds).NotEmpty();
        }
    }
}