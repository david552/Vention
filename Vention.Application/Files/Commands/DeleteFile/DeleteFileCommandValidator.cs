using FluentValidation;

namespace Vention.Application.Files.Commands.DeleteFile
{
    public sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
    {
        public DeleteFileCommandValidator()
        {
            RuleFor(x => x.FileId)
                .NotEmpty();

            RuleFor(x => x.OrganizationId)
                .NotEmpty();

            RuleFor(x => x.ActingUserId)
                .NotEmpty();
        }
    }
}