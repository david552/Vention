using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Users;

namespace Vention.Application.Users.Commands.DeleteUser
{
    public sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteUserCommand command, CancellationToken ct)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(command.Id), ct)
                ?? throw new NotFoundException($"User '{command.Id}' was not found.");

            user.Delete();
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
