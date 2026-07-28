using FluentValidation;

namespace Vention.Application.Users.Commands.DeleteUser
{
    public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.ActingUserId).NotEmpty();
        }
    }
}
