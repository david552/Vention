using FluentValidation;

namespace Vention.Application.Messages.Queries.GetChatMessageById
{
    public sealed class GetChatMessageByIdQueryValidator : AbstractValidator<GetChatMessageByIdQuery>
    {
        public GetChatMessageByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
