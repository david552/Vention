using FluentValidation;

namespace Vention.Application.Organizations.Queries.GetOrganizationById
{
    public sealed class GetOrganizationByIdQueryValidator : AbstractValidator<GetOrganizationByIdQuery>
    {
        public GetOrganizationByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
