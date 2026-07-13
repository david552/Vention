using Microsoft.AspNetCore.Mvc;
using Vention.Application.Chats.Commands.CreateChatSession;
using Vention.Application.Chats.Commands.DeleteChatSession;
using Vention.Application.Chats.Commands.GetOrCreateDirectChatSession;
using Vention.Application.Chats.Commands.RenameChatSession;
using Vention.Application.Chats.Contracts;
using Vention.Application.Chats.Queries.GetChatSessionById;
using Vention.Application.Chats.Queries.GetChatSessionMembers;
using Vention.Application.Chats.Queries.GetChatSessionsByOrganization;
using Vention.Application.Chats.Queries.GetSessionsForUser;
using Vention.Application.Messaging;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("api/chat-sessions")]
    public sealed class ChatSessionsController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        public ChatSessionsController(IDispatcher dispatcher) => _dispatcher = dispatcher;

        [HttpPost]
        public async Task<ActionResult<ChatSessionResponse>> Create(
            CreateChatSessionCommand command,
            CancellationToken ct)
        {
            var result = await _dispatcher.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }


        [HttpPost("direct")]
        public async Task<ActionResult<ChatSessionResponse>> GetOrCreateDirect(
            GetOrCreateDirectChatSessionCommand command,
            CancellationToken ct)
        {
            var result = await _dispatcher.Send(command, ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ChatSessionResponse>> GetById(Guid id, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetChatSessionByIdQuery(id), ct);
            return Ok(result);
        }

        [HttpGet("by-user/{userId:guid}")]
        public async Task<ActionResult<IReadOnlyList<ChatSessionResponse>>> GetByUser(
            Guid userId,
            [FromQuery] Guid organizationId,
            CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetSessionsForUserQuery(userId, organizationId), ct);
            return Ok(result);
        }

        [HttpGet("by-organization/{organizationId:guid}")]
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
            var result = await _dispatcher.Send(new GetChatSessionMembersQuery(id), ct);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ChatSessionResponse>> Rename(
            Guid id,
            [FromBody] RenameChatSessionRequest request,
            CancellationToken ct)
        {
            var result = await _dispatcher.Send(new RenameChatSessionCommand(id, request.Title), ct);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _dispatcher.Send(new DeleteChatSessionCommand(id), ct);
            return NoContent();
        }
    }

    public sealed record RenameChatSessionRequest(string Title);
}
