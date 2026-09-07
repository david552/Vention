using Microsoft.AspNetCore.Mvc;
using Vention.API.Authorization;
using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Chats.Commands.DeleteChatSession;
using Vention.Application.Chats.Commands.GetOrCreateDirectChatSession;
using Vention.Application.Chats.Commands.MarkChatSessionAsRead;
using Vention.Application.Chats.Commands.RenameChatSession;
using Vention.Application.Chats.Contracts;
using Vention.Application.Chats.Queries.GetChatSessionById;
using Vention.Application.Chats.Queries.GetSessionsForUser;
using Vention.Application.Common;
using Vention.Application.Messaging;
using Vention.Domain.Membership;
using Vention.Presentation.Common.Extensions;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("chats/sessions")]
    public sealed class ChatSessionsController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        private readonly ActiveOrganizationContextService _activeOrg;
        private readonly ICurrentUserService _currentUser;


        public ChatSessionsController(
            IDispatcher dispatcher,
            ActiveOrganizationContextService activeOrg,
            ICurrentUserService currentUser)
        {
            _dispatcher = dispatcher;
            _activeOrg = activeOrg;
            _currentUser = currentUser;

        }
        [HttpPost]
        public async Task<ActionResult<ChatSessionResponse>> Create(
           CreateChatSessionRequest request,
           CancellationToken ct)
        {
            var userId = _currentUser.UserId;


            var organizationId = request.OrganizationId != Guid.Empty
                ? request.OrganizationId
                : Request.GetRequiredOrganizationId();

            await _activeOrg.EnsureIsMemberAsync(userId, organizationId, ct);


            var command = new GetOrCreateDirectChatSessionCommand(
               organizationId,
               userId,
               request.ParticipantUserId);

            var result = await _dispatcher.Send(command, ct);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ChatSessionResponse>> GetById(Guid id, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetChatSessionByIdQuery(id, _currentUser.UserId), ct);
            return Ok(result);
        }


        [HttpGet]
        [RequireActiveOrganizationRole(
            MembershipRole.Owner, MembershipRole.Admin, MembershipRole.Editor,
            MembershipRole.Member, MembershipRole.Viewer)]
        public async Task<IActionResult> GetForActiveOrganization(
            [FromQuery] bool paginated = false,
            [FromQuery] bool all = false,
            [FromQuery] string? cursor = null,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var userId = _currentUser.UserId;
            var organizationId = Request.GetRequiredOrganizationId();
            var usePagination = paginated && !all;

            var result = await _dispatcher.Send(
                new GetChatSessionsForUserQuery(userId, organizationId, usePagination, cursor, pageSize), ct);

            if (result.Paginated)
                return Ok(new CursorPage<ChatSessionResponse>(result.Items, result.NextCursor));

            return Ok(result.Items);
        }

        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
        {
            await _dispatcher.Send(
                new MarkChatSessionAsReadCommand(id, _currentUser.UserId),
                ct);

            return NoContent();
        }

        [HttpPut("{id:guid}")]
        [HttpPatch("{id:guid}")]
        public async Task<ActionResult<ChatSessionResponse>> Rename(
            Guid id,
            [FromBody] RenameChatSessionRequest request,
            CancellationToken ct)
        {
            var result = await _dispatcher.Send(new RenameChatSessionCommand(id, request.Title, _currentUser.UserId), ct);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _dispatcher.Send(new DeleteChatSessionCommand(id, _currentUser.UserId), ct);
            return NoContent();
        }
    }

    public sealed record CreateChatSessionRequest(Guid ParticipantUserId, Guid OrganizationId = default);

    public sealed record RenameChatSessionRequest(string Title);

}
