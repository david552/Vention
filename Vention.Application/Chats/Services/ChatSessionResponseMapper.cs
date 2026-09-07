using Vention.Application.Chats.Contracts;
using Vention.Domain.Chats;
using Vention.Domain.Messages;
using Vention.Domain.Users;

namespace Vention.Application.Chats.Services
{

    public sealed class ChatSessionResponseMapper
    {
        private readonly IChatSessionMemberRepository _memberRepository;
        private readonly IChatMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public ChatSessionResponseMapper(
            IChatSessionMemberRepository memberRepository,
            IChatMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            _memberRepository = memberRepository;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public async Task<ChatSessionResponse> MapAsync(
            ChatSession session,
            UserId currentUserId,
            CancellationToken ct)
        {
            var members = await _memberRepository.GetBySessionIdAsync(session.Id, ct);

            var myMembership = members.First(m => m.UserId == currentUserId);

            var otherMemberId = members
                .Select(m => m.UserId)
                .First(id => id != currentUserId);

            var otherUser = await _userRepository.GetByIdAsync(otherMemberId, ct)
                ?? throw new InvalidOperationException("Participant user not found.");

            var lastMessage = await _messageRepository.GetLatestBySessionIdAsync(session.Id, ct);

            var unreadCount = await _messageRepository.CountUnreadAsync(
                session.Id,
                currentUserId,
                myMembership.LastReadAt,
                ct);

            return new ChatSessionResponse(
                Id: session.Id.Value,
                Participant: new ChatParticipantResponse(otherUser.Id.Value, otherUser.Name),
                LastMessage: lastMessage?.Content ?? string.Empty,
                LastMessageAt: lastMessage?.CreatedAt ?? session.UpdatedAt,
                UnreadCount: unreadCount);
        }

        public async Task<IReadOnlyList<ChatSessionResponse>> MapManyAsync(
            IReadOnlyList<ChatSession> sessions,
            UserId currentUserId,
            CancellationToken ct)
        {
            var result = new List<ChatSessionResponse>(sessions.Count);

            foreach (var session in sessions)
                result.Add(await MapAsync(session, currentUserId, ct));

            return result;
        }
    }
}