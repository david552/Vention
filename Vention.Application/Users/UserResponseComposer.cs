using Mapster;
using Vention.Application.Users.Contracts;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Users
{
    public sealed class UserResponseComposer
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly IOrganizationRepository _organizationRepository;

        public UserResponseComposer(
            IMembershipRepository membershipRepository,
            IOrganizationRepository organizationRepository)
        {
            _membershipRepository = membershipRepository;
            _organizationRepository = organizationRepository;
        }

        public async Task<UserResponse> ComposeAsync(User user, CancellationToken ct)
        {
            var organisations = await BuildOrganisationsAsync(user.Id, ct);
            return user.Adapt<UserResponse>() with { Organisations = organisations };
        }

        public async Task<IReadOnlyList<UserResponse>> ComposeManyAsync(
            IReadOnlyList<User> users,
            CancellationToken ct)
        {
            var result = new List<UserResponse>(users.Count);
            foreach (var user in users)
                result.Add(await ComposeAsync(user, ct));

            return result;
        }

        private async Task<IReadOnlyList<UserOrganizationMembershipResponse>> BuildOrganisationsAsync(
            UserId userId,
            CancellationToken ct)
        {
            var memberships = await _membershipRepository.GetByUserIdAsync(userId, ct);
            var organisations = new List<UserOrganizationMembershipResponse>(memberships.Count);

            foreach (var membership in memberships)
            {
                var organization = await _organizationRepository.GetByIdAsync(membership.OrganizationId, ct);
                if (organization is null)
                    continue;

                organisations.Add(new UserOrganizationMembershipResponse(
                    organization.Id.Value,
                    organization.Name,
                    membership.Role.ToString().ToUpperInvariant()));
            }

            return organisations;
        }
    }
}