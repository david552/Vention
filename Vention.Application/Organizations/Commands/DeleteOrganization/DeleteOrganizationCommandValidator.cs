using FluentValidation;

namespace Vention.Application.Organizations.Commands.DeleteOrganization
{
    public sealed class DeleteOrganizationCommandValidator : AbstractValidator<DeleteOrganizationCommand>
    {
        public DeleteOrganizationCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
