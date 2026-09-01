using Microsoft.AspNetCore.Mvc;
using Vention.API.Authorization;
using Vention.Presentation.Common.Extensions;
using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Chats.Commands.CreateChatSession;
using Vention.Application.Chats.Commands.DeleteChatSession;
using Vention.Application.Chats.Commands.GetOrCreateDirectChatSession;
using Vention.Application.Chats.Commands.RenameChatSession;
using Vention.Application.Chats.Contracts;
using Vention.Application.Chats.Queries.GetChatSessionById;
using Vention.Application.Chats.Queries.GetChatSessionMembers;
using Vention.Application.Chats.Queries.GetChatSessionsByOrganization;
using Vention.Application.Chats.Queries.GetSessionsForUser;
using Vention.Application.Common;
using Vention.Application.Messaging;
using Vention.Domain.Membership;

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


            var command = new CreateChatSessionCommand(
               request.OrganizationId,
               userId,
               request.ParticipantUserId);

            var result = await _dispatcher.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }


        [HttpPost("direct")]
        public async Task<ActionResult<ChatSessionResponse>> GetOrCreateDirect(
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

        [HttpGet("by-user/{userId:guid}")]
        public async Task<ActionResult<CursorPage<ChatSessionResponse>>> GetByUser(
           Guid userId,
           [FromQuery] Guid organizationId,
           [FromQuery] string? cursor,
           [FromQuery] int pageSize = 50,
           CancellationToken ct = default)
        {
            if (userId != _currentUser.UserId)
                return Forbid();

            await _activeOrg.EnsureIsMemberAsync(userId, organizationId, ct);

            var result = await _dispatcher.Send(
                    new GetSessionsForUserQuery(userId, organizationId, cursor, pageSize), ct);

            return Ok(result);
        }

        [HttpGet("by-organization/{organizationId:guid}")]
        [RequireActiveOrganizationRole(
        MembershipRole.Owner, MembershipRole.Admin, MembershipRole.Editor,
        MembershipRole.Member, MembershipRole.Viewer)]
        public async Task<ActionResult<IReadOnlyList<ChatSessionResponse>>> GetByOrganization(
            Guid organizationId,
            CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetChatSessionsByOrganizationQuery(organizationId), ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}/members")]
        public async Task<ActionResult<IReadOnlyList<ChatSessionMemberResponse>>> GetMembers(
            Guid id,
            CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetChatSessionMembersQuery(id, _currentUser.UserId), ct);
            return Ok(result);
        }

        [HttpGet]
        [RequireActiveOrganizationRole(
        MembershipRole.Owner, MembershipRole.Admin, MembershipRole.Editor,
        MembershipRole.Member, MembershipRole.Viewer)]
        public async Task<ActionResult<CursorPage<ChatSessionResponse>>> GetForActiveOrganization(
        [FromQuery] string? cursor,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        {
            var userId = _currentUser.UserId;
            var organizationId = Request.GetRequiredOrganizationId();
            var result = await _dispatcher.Send(
                new GetSessionsForUserQuery(userId, organizationId, cursor, pageSize), ct);
            return Ok(result);
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

    public sealed record CreateChatSessionRequest(Guid OrganizationId, Guid ParticipantUserId);

    public sealed record RenameChatSessionRequest(string Title);

}
