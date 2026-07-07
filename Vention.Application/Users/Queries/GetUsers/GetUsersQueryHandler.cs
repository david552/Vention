using Mapster;
using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;
using Vention.Domain.Users;

namespace Vention.Application.Users.Queries.GetUsers
{
    public sealed class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, IReadOnlyList<UserResponse>>
    {
        private readonly IUserRepository _userRepository;
        public GetUsersQueryHandler(IUserRepository userRepository) => _userRepository = userRepository;

        public async Task<IReadOnlyList<UserResponse>> Handle(GetUsersQuery query, CancellationToken ct)
        {
            var users = await _userRepository.GetAllAsync(ct);
            return users.Adapt<IReadOnlyList<UserResponse>>();
        }
    }
}
