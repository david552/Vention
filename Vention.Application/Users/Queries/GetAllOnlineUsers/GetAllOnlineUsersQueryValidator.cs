using FluentValidation;

namespace Vention.Application.Users.Queries.GetAllOnlineUsers
{

    public sealed class GetAllOnlineUsersQueryValidator : AbstractValidator<GetAllOnlineUsersQuery>
    {
        public GetAllOnlineUsersQueryValidator()
        {
            RuleFor(x => x.ActingUserId).NotEmpty();
        }
    }
}