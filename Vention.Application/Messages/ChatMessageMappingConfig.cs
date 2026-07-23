using Mapster;
using Vention.Application.Messages.Contracts;
using Vention.Domain.Messages;

namespace Vention.Application.Messages
{
    public sealed class ChatMessageMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ChatMessage, ChatMessageResponse>()
                .Map(dest => dest.Id, src => src.Id.Value)
                .Map(dest => dest.ChatSessionId, src => src.ChatSessionId.Value)
                .Map(dest => dest.SenderId, src => src.SenderId.Value);
        }
    }
}
