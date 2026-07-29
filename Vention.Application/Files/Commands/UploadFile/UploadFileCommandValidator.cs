using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vention.Application.Files.Commands.UploadFile
{
    public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
    {
        public UploadFileCommandValidator()
        {
            RuleFor(x => x.Filename)
                .NotEmpty()
                .MaximumLength(FileUploadRules.MaxFilenameLength);

            RuleFor(x => x.ContentType)
                .NotEmpty()
                .Must(FileUploadRules.IsAllowedContentType)
                .WithMessage("Only PDF, Word and plain text documents are allowed.");

            RuleFor(x => x.Size)
                .GreaterThan(0)
                .LessThanOrEqualTo(FileUploadRules.MaxFileSizeBytes)
                .WithMessage($"File size cannot exceed {FileUploadRules.MaxFileSizeBytes / (1024 * 1024)}MB.")
                .When(x => x.Size.HasValue);

            RuleFor(x => x.OrganizationId)
                .NotEmpty();

            RuleFor(x => x.ActingUserId)
                .NotEmpty();
        }
    }
}
