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

            organization.Delete();


            var memberships = await _membershipRepository.GetByOrganizationIdAsync(organization.Id, ct);
            foreach (var membership in memberships)
                _membershipRepository.Remove(membership);

            var chatSessions = await _chatSessionRepository.GetByOrganizationIdAsync(organization.Id, ct);
            foreach (var chatSession in chatSessions)
                _chatSessionRepository.Remove(chatSession);

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
