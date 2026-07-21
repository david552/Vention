using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;
using Vention.Domain.Membership;
using Vention.Domain.Users;

namespace Vention.Application.Users.Commands.UpdateUser
{
    public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserAuthorizationService _authService;

        public UpdateUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IMembershipRepository membershipRepository,
            UserAuthorizationService authService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _membershipRepository = membershipRepository;
            _authService = authService;
        }

        public async Task<UserResponse> Handle(UpdateUserCommand command, CancellationToken ct)
        {
            await _authService.EnsureCanManageUserAsync(command.Id, command.ActingUserId, ct);

            var user = await _userRepository.GetByIdAsync(new UserId(command.Id), ct)
                ?? throw new NotFoundException($"User '{command.Id}' was not found.");

            user.UpdateProfile(command.Name);
            await _unitOfWork.SaveChangesAsync(ct);

            return user.Adapt<UserResponse>();
        }
    }
}
