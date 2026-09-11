using Mapster;

using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;
using Vention.Domain.Membership;
using Vention.Domain.Users;

namespace Vention.Application.Users.Commands.CreateUser
{
    public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMembershipRepository _membershipRepository;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IMembershipRepository membershipRepository)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _membershipRepository = membershipRepository;
        }

        public async Task<UserResponse> Handle(CreateUserCommand command, CancellationToken ct)
        {
            if (!command.CreatedByUserId.HasValue)
                throw new ForbiddenException("Authentication required to create users.");

            var actingMemberships = await _membershipRepository.GetByUserIdAsync(
                new UserId(command.CreatedByUserId.Value), ct);

            if (!actingMemberships.Any(m => MembershipRoleRules.IsOwnerOrAdmin(m.Role)))
                throw new ForbiddenException("Only an Owner or Admin can create users.");

            var email = Email.Create(command.Email);

            if (await _userRepository.ExistsByEmailAsync(email, ct))
                throw new InvalidOperationException($"A user with email '{command.Email}' already exists.");

            string passwordHash = _passwordHasher.Hash(command.Password);

            UserId? createdByUserId = command.CreatedByUserId.HasValue
                ? new UserId(command.CreatedByUserId.Value)
                : null;

            var user = User.Create(email, command.Name, passwordHash, createdByUserId);

            _userRepository.Add(user);
            await _unitOfWork.SaveChangesAsync(ct);

            return user.Adapt<UserResponse>();
        }
    }
}
