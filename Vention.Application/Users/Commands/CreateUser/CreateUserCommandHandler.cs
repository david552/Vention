using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;
using Vention.Domain.Users;

namespace Vention.Application.Users.Commands.CreateUser
{
    public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;

        public CreateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserResponse> Handle(CreateUserCommand command, CancellationToken ct)
        {
            var email = Email.Create(command.Email);

            if (await _userRepository.ExistsByEmailAsync(email, ct))
                throw new InvalidOperationException($"A user with email '{command.Email}' already exists.");

            string passwordHash = _passwordHasher.Hash(command.Password);

            var user = User.Create(email, command.Name, passwordHash);

            _userRepository.Add(user);
            await _unitOfWork.SaveChangesAsync(ct);

            return user.Adapt<UserResponse>();
        }
    }
}
