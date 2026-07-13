using Mapster;
using Vention.Application.Chats.Contracts;
using Vention.Domain.Chats;

namespace Vention.Application.Chats
{
    public sealed class ChatSessionMemberMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ChatSessionMember, ChatSessionMemberResponse>()
                .Map(dest => dest.Id, src => src.Id.Value)
                .Map(dest => dest.ChatSessionId, src => src.ChatSessionId.Value)
                .Map(dest => dest.UserId, src => src.UserId.Value);
        }
    }
}
