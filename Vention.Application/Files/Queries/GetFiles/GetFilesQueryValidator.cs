using FluentValidation;

namespace Vention.Application.Files.Queries.GetFiles
{
    public sealed class GetFilesQueryValidator : AbstractValidator<GetFilesQuery>
    {
        public GetFilesQueryValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty();

            RuleFor(x => x.Limit)
                .GreaterThan(0)
                .LessThanOrEqualTo(500);
        }
    }
}