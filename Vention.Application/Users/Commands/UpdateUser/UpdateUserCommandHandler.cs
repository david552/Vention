using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;
using Vention.Domain.Users;

namespace Vention.Application.Users.Commands.UpdateUser
{
    public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserResponse> Handle(UpdateUserCommand command, CancellationToken ct)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(command.Id), ct)
                ?? throw new NotFoundException($"User '{command.Id}' was not found.");

            user.UpdateProfile(command.Name);
            await _unitOfWork.SaveChangesAsync(ct);

            return user.Adapt<UserResponse>();
        }
    }
}
