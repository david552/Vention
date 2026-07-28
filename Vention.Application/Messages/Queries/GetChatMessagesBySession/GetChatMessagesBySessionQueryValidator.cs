using FluentValidation;

namespace Vention.Application.Messages.Queries.GetChatMessagesBySession
{
    public sealed class GetChatMessagesBySessionQueryValidator
        : AbstractValidator<GetChatMessagesBySessionQuery>
    {
        public GetChatMessagesBySessionQueryValidator()
        {
            RuleFor(x => x.SessionId).NotEmpty();
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
