using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Membership;
using Vention.Domain.Users;

namespace Vention.Application.Users.Commands.DeleteUser
{
    public sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserAuthorizationService _authService;


        public DeleteUserCommandHandler(
            IUserRepository userRepository,
            IMembershipRepository membershipRepository,
            IUnitOfWork unitOfWork,
            UserAuthorizationService authService)
        {
            _userRepository = userRepository;
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
            _authService = authService;
        }

        public async Task Handle(DeleteUserCommand command, CancellationToken ct)
        {
            await _authService.EnsureCanManageUserAsync(command.Id, command.ActingUserId, ct);

            var user = await _userRepository.GetByIdAsync(new UserId(command.Id), ct)
                ?? throw new NotFoundException($"User '{command.Id}' was not found.");

            user.Delete();

            var memberships = await _membershipRepository.GetByUserIdAsync(user.Id, ct);

            foreach (var membership in memberships)
                _membershipRepository.Remove(membership);

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
