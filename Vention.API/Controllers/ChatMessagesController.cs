using Microsoft.AspNetCore.Mvc;
using Vention.API.Extensions;
using Vention.Application.Common;
using Vention.Application.Messages.Commands.DeleteChatMessage;
using Vention.Application.Messages.Commands.SendChatMessage;
using Vention.Application.Messages.Contracts;
using Vention.Application.Messages.Queries.GetChatMessageById;
using Vention.Application.Messages.Queries.GetChatMessagesBySession;
using Vention.Application.Messaging;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("chats/sessions/{sessionId:guid}/messages")]
    public sealed class ChatMessagesController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        public ChatMessagesController(IDispatcher dispatcher) => _dispatcher = dispatcher;

        [HttpPost]
        public async Task<ActionResult<ChatMessageResponse>> Send(
            Guid sessionId,
            [FromBody] SendChatMessageRequest request,
            CancellationToken ct)
        {
            var command = new SendChatMessageCommand(sessionId, User.GetUserId(), request.Content);

            var result = await _dispatcher.Send(command, ct);

            return CreatedAtAction(nameof(GetById), new { sessionId, id = result.Id }, result);
        }

        [HttpGet]
        public async Task<ActionResult<CursorPage<ChatMessageResponse>>> GetBySession(
            Guid sessionId,
            [FromQuery] string? cursor = null,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {

            var result = await _dispatcher.Send(
                new GetChatMessagesBySessionQuery(sessionId, User.GetUserId(), cursor, pageSize), ct);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ChatMessageResponse>> GetById(
            Guid sessionId,
            Guid id,
            CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetChatMessageByIdQuery(id, User.GetUserId()), ct);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            Guid sessionId,
            Guid id,
            CancellationToken ct)
        {
            await _dispatcher.Send(new DeleteChatMessageCommand(id, User.GetUserId()), ct);
            return NoContent();
        }
    }

    public sealed record SendChatMessageRequest(string Content);
}
