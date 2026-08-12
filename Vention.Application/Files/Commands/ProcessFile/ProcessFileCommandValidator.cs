using FluentValidation;

namespace Vention.Application.Files.Commands.ProcessFile
{
    public sealed class ProcessFileCommandValidator : AbstractValidator<ProcessFileCommand>
    {
        public ProcessFileCommandValidator()
        {
            RuleFor(x => x.FileId).NotEmpty();
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.ActingUserId).NotEmpty();
        }
    }
}