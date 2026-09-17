using FluentValidation;

namespace Vention.Application.Rag.Commands.AskQuestion
{
    public sealed class AskQuestionCommandValidator : AbstractValidator<AskQuestionCommand>
    {
        public AskQuestionCommandValidator()
        {
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.ActingUserId).NotEmpty();
            RuleFor(x => x.Question)
                .NotEmpty()
                .MaximumLength(4000);

            RuleFor(x => x.FileId)
              .Must(id => id is null || id != Guid.Empty)
              .WithMessage("FileId cannot be an empty GUID.");
        }
    }
}