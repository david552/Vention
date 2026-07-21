using FluentValidation;
using Vention.Domain.Membership;

namespace Vention.Application.Membership.Commands.ChangeMembershipRole
{
    public sealed class ChangeMembershipRoleCommandValidator : AbstractValidator<ChangeMembershipRoleCommand>
    {
        public ChangeMembershipRoleCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.ActingUserId).NotEmpty();

            RuleFor(x => x.Role)
                .NotEmpty()
                .Must(role => Enum.TryParse<MembershipRole>(role, ignoreCase: true, out _))
                .WithMessage("Role must be one of: Owner, Admin, Member.");
        }
    }
}
