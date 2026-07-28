using FluentValidation;

namespace Vention.Application.Organizations.Commands.UpdateOrganization
{
    public sealed class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
    {
        public UpdateOrganizationCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        }
    }
}
