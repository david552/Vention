using Vention.Application.Authorization;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;
using Vention.Domain.Users;

namespace Vention.Application.Users.Queries.GetUserById
{
    public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly UserAuthorizationService _authService;
        private readonly UserResponseComposer _composer;

        public GetUserByIdQueryHandler(
           IUserRepository userRepository,
           UserAuthorizationService authService,
           UserResponseComposer composer)
        {
            _userRepository = userRepository;
            _authService = authService;
            _composer = composer;
        }

        public async Task<UserResponse> Handle(GetUserByIdQuery query, CancellationToken ct)
        {
            await _authService.EnsureCanViewUserAsync(query.Id, query.ActingUserId, ct);
            var user = await _userRepository.GetByIdAsync(new UserId(query.Id), ct)
                ?? throw new NotFoundException($"User '{query.Id}' was not found.");
            return await _composer.ComposeAsync(user, ct);
        }
    }
}
