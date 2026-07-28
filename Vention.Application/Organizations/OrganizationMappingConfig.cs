using Mapster;
using Vention.Application.Organizations.Contracts;
using Vention.Domain.Organizations;

namespace Vention.Application.Organizations
{
    public sealed class OrganizationMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Organization, OrganizationResponse>()
                .Map(dest => dest.Id, src => src.Id.Value);
        }
    }
}
