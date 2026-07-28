using FluentValidation;

namespace Vention.Application.Chats.Queries.GetChatSessionById
{
    public sealed class GetChatSessionByIdQueryValidator : AbstractValidator<GetChatSessionByIdQuery>
    {
        public GetChatSessionByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
