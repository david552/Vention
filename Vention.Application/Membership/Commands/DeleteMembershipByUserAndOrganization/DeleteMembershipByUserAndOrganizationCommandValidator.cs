using FluentValidation;

namespace Vention.Application.Membership.Commands.DeleteMembershipByUserAndOrganization
{
    public sealed class DeleteMembershipByUserAndOrganizationCommandValidator : AbstractValidator<DeleteMembershipByUserAndOrganizationCommand>
    {
        public DeleteMembershipByUserAndOrganizationCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.ActingUserId).NotEmpty();
        }
    }
}
