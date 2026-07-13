using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;

namespace Vention.Application.Membership.Commands.DeleteMembership
{
    public sealed class DeleteMembershipCommandHandler : ICommandHandler<DeleteMembershipCommand>
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteMembershipCommandHandler(IMembershipRepository membershipRepository, IUnitOfWork unitOfWork)
        {
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteMembershipCommand command, CancellationToken ct)
        {
            var membership = await _membershipRepository.GetByIdAsync(new MembershipId(command.Id), ct)
                ?? throw new NotFoundException($"Membership '{command.Id}' was not found.");

            _membershipRepository.Remove(membership);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
