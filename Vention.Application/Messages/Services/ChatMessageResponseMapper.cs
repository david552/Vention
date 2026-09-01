using Vention.Application.Messages.Contracts;
using Vention.Domain.Messages;
using Vention.Domain.Users;

namespace Vention.Application.Messages.Services
{

    public static class ChatMessageResponseMapper
    {
        public static ChatMessageResponse Map(
            ChatMessage message,
            User sender,
            UserId requestingUserId)
        {
            return new ChatMessageResponse(
                Id: message.Id.Value,
                ChatId: message.ChatSessionId.Value,
                Content: message.Content,
                SenderId: message.SenderId.Value,
                SenderName: sender.Name,
                CreatedAt: message.CreatedAt,
                IsOwn: message.SenderId == requestingUserId);
        }

        public static IReadOnlyList<ChatMessageResponse> MapMany(
            IReadOnlyList<ChatMessage> messages,
            IReadOnlyDictionary<UserId, User> usersById,
            UserId requestingUserId)
        {
            return messages
                .Select(message => Map(message, usersById[message.SenderId], requestingUserId))
                .ToList();
        }
    }
}