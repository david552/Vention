using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Organizations;

namespace Vention.Application.Chats.Queries.GetChatSessionsByOrganization
{
    public sealed class GetChatSessionsByOrganizationQueryHandler
        : IQueryHandler<GetChatSessionsByOrganizationQuery, IReadOnlyList<ChatSessionResponse>>
    {
        private readonly IChatSessionRepository _chatSessionRepository;
        public GetChatSessionsByOrganizationQueryHandler(IChatSessionRepository chatSessionRepository) => _chatSessionRepository = chatSessionRepository;

        public async Task<IReadOnlyList<ChatSessionResponse>> Handle(GetChatSessionsByOrganizationQuery query, CancellationToken ct)
        {
            var chatSessions = await _chatSessionRepository.GetByOrganizationIdAsync(new OrganizationId(query.OrganizationId), ct);
            return chatSessions.Adapt<IReadOnlyList<ChatSessionResponse>>();
        }
    }

}
