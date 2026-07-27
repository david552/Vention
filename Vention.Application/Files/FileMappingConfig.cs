using Mapster;
using Vention.Application.Files.Contracts;
using Vention.Domain.Files;

namespace Vention.Application.Files
{
    public sealed class FileMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<StoredFile, FileResponse>()
                .Map(dest => dest.Id, src => src.Id.Value)
                .Map(dest => dest.OrganisationId, src => src.OrganizationId.Value)
                .Map(dest => dest.OwnerId, src => src.OwnerId.Value)
                .Map(dest => dest.Status, src => src.Status.ToString().ToLowerInvariant());
        }
    }
}
