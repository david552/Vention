using Mapster;
using Vention.Application.Chats.Contracts;
using Vention.Domain.Chats;

namespace Vention.Application.Chats
{
    public sealed class ChatSessionMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ChatSession, ChatSessionResponse>()
                .Map(dest => dest.Id, src => src.Id.Value)
                .Map(dest => dest.OrganizationId, src => src.OrganizationId.Value)
                .Map(dest => dest.CreatedByUserId, src => src.CreatedByUserId.Value);
        }
    }
}
