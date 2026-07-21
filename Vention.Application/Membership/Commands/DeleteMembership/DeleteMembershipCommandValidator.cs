using FluentValidation;

namespace Vention.Application.Membership.Commands.DeleteMembership
{
    public sealed class DeleteMembershipCommandValidator : AbstractValidator<DeleteMembershipCommand>
    {
        public DeleteMembershipCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.ActingUserId).NotEmpty();
        }
    }
}
