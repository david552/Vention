using FluentValidation;

namespace Vention.Application.Organizations.Queries.GetOrganizationsByIds
{

    public sealed class GetOrganizationsByIdsQueryValidator
        : AbstractValidator<GetOrganizationsByIdsQuery>
    {
        public GetOrganizationsByIdsQueryValidator()
        {
            RuleFor(x => x.ActingUserId).NotEmpty();
            RuleFor(x => x.Ids).NotNull();
            RuleForEach(x => x.Ids).NotEmpty();
        }
    }
}