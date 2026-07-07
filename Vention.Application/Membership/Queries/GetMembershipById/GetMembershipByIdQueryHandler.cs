using Mapster;
using Vention.Application.Exceptions;
using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Membership;

namespace Vention.Application.Membership.Queries.GetMembershipById
{
    public sealed class GetMembershipByIdQueryHandler : IQueryHandler<GetMembershipByIdQuery, MembershipResponse>
    {
        private readonly IMembershipRepository _membershipRepository;
        public GetMembershipByIdQueryHandler(IMembershipRepository membershipRepository) => _membershipRepository = membershipRepository;

        public async Task<MembershipResponse> Handle(GetMembershipByIdQuery query, CancellationToken ct)
        {
            var membership = await _membershipRepository.GetByIdAsync(new MembershipId(query.Id), ct)
                ?? throw new NotFoundException($"Membership '{query.Id}' was not found.");

            return membership.Adapt<MembershipResponse>();
        }
    }
}
