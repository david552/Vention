using Mapster;
using Vention.Application.Authorization;
using Vention.Application.Exceptions;
using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Membership;

namespace Vention.Application.Membership.Queries.GetMembershipById
{
    public sealed class GetMembershipByIdQueryHandler : IQueryHandler<GetMembershipByIdQuery, MembershipResponse>
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly OrganizationAuthorizationService _orgAuth;

        public GetMembershipByIdQueryHandler(
            IMembershipRepository membershipRepository,
            OrganizationAuthorizationService orgAuth)
        {
            _membershipRepository = membershipRepository;
            _orgAuth = orgAuth;
        } 

        public async Task<MembershipResponse> Handle(GetMembershipByIdQuery query, CancellationToken ct)
        {
            var membership = await _membershipRepository.GetByIdAsync(new MembershipId(query.Id), ct)
                ?? throw new NotFoundException($"Membership '{query.Id}' was not found.");

            await _orgAuth.EnsureIsOrganizationMemberAsync(query.ActingUserId, membership.OrganizationId.Value, ct);

            return membership.Adapt<MembershipResponse>();
        }
    }
}
