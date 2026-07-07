using Mapster;
using Vention.Application.Membership.Contracts;
using DomainMembership = Vention.Domain.Membership.Membership;

namespace Vention.Application.Membership
{
    public sealed class MembershipMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<DomainMembership, MembershipResponse>()
                .Map(dest => dest.Id, src => src.Id.Value)
                .Map(dest => dest.UserId, src => src.UserId.Value)
                .Map(dest => dest.OrganizationId, src => src.OrganizationId.Value)
                .Map(dest => dest.Role, src => src.Role.ToString());
        }
    }
}
