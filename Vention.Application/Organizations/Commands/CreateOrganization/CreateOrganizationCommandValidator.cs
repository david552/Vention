using FluentValidation;

namespace Vention.Application.Organizations.Commands.CreateOrganization
{
    public sealed class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
    {
        public CreateOrganizationCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
