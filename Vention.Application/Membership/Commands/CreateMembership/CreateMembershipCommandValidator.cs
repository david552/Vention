using FluentValidation;
using Vention.Domain.Membership;

namespace Vention.Application.Membership.Commands.CreateMembership
{
    public sealed class CreateMembershipCommandValidator : AbstractValidator<CreateMembershipCommand>
    {
        public CreateMembershipCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.ActingUserId).NotEmpty();

            RuleFor(x => x.Role)
                .NotEmpty()
                .Must(role => Enum.TryParse<MembershipRole>(role, ignoreCase: true, out _))
                .WithMessage("Role must be one of: Owner, Admin, Member.");
        }
    }
}
