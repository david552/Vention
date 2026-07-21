using FluentValidation;

namespace Vention.Application.Chats.Queries.GetChatSessionsByOrganization
{
    public sealed class GetChatSessionsByOrganizationQueryValidator
        : AbstractValidator<GetChatSessionsByOrganizationQuery>
    {
        public GetChatSessionsByOrganizationQueryValidator()
        {
            RuleFor(x => x.OrganizationId).NotEmpty();
        }
    }
}
