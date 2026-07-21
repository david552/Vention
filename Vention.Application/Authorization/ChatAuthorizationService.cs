using Vention.Application.Exceptions;
using Vention.Domain.Chats;
using Vention.Domain.Users;

namespace Vention.Application.Authorization
{
    public sealed class ChatAuthorizationService
    {
        private readonly IChatSessionMemberRepository _memberRepository;
        private readonly IChatSessionRepository _sessionRepository;

        public ChatAuthorizationService(
            IChatSessionMemberRepository memberRepository,
            IChatSessionRepository sessionRepository)
        {
            _memberRepository = memberRepository;
            _sessionRepository = sessionRepository;
        }

        public async Task EnsureSessionExistsAsync(Guid sessionId, CancellationToken ct)
        {
            if (!await _sessionRepository.ExistsByIdAsync(new ChatSessionId(sessionId), ct))
                throw new NotFoundException($"Chat session '{sessionId}' was not found.");
        }

        public async Task EnsureIsSessionMemberAsync(
            Guid sessionId,
            Guid userId,
            CancellationToken ct)
        {
            await EnsureSessionExistsAsync(sessionId, ct);

            if (!await _memberRepository.IsMemberAsync(
                    new ChatSessionId(sessionId),
                    new UserId(userId),
                    ct))
            {
                throw new ForbiddenException(
                    $"User '{userId}' is not a participant of chat session '{sessionId}'.");
            }
        }
    }
}