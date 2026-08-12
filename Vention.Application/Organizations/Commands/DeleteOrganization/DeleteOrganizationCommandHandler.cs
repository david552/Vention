using System.Data;
using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;

namespace Vention.Application.Organizations.Commands.DeleteOrganization
{
    public sealed class DeleteOrganizationCommandHandler : ICommandHandler<DeleteOrganizationCommand>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteOrganizationCommandHandler(
            IOrganizationRepository organizationRepository,
            IMembershipRepository membershipRepository,
            IChatSessionRepository chatSessionRepository,
            IUnitOfWork unitOfWork)
        {
            _organizationRepository = organizationRepository;
            _membershipRepository = membershipRepository;
            _chatSessionRepository = chatSessionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteOrganizationCommand command, CancellationToken ct)
        {
            var organization = await _organizationRepository.GetByIdAsync(new OrganizationId(command.Id), ct)
                ?? throw new NotFoundException($"Organization '{command.Id}' was not found.");

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            try
            {
                await _membershipRepository.DeleteByOrganizationIdAsync(organization.Id, ct);
                await _chatSessionRepository.DeleteByOrganizationIdAsync(organization.Id, ct);

                organization.Delete();

                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }
    }
}
