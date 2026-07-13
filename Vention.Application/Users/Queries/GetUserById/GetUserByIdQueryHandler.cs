using Mapster;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;
using Vention.Domain.Users;

namespace Vention.Application.Users.Queries.GetUserById
{
    public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserResponse>
    {
        private readonly IUserRepository _userRepository;
        public GetUserByIdQueryHandler(IUserRepository userRepository) => _userRepository = userRepository;

        public async Task<UserResponse> Handle(GetUserByIdQuery query, CancellationToken ct)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(query.Id), ct)
                ?? throw new NotFoundException($"User '{query.Id}' was not found.");

            return user.Adapt<UserResponse>();
        }
    }
}
