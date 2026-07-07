using Microsoft.AspNetCore.Mvc;
using Vention.Application.Chats.Commands.DeleteChatMessage;
using Vention.Application.Chats.Commands.SendChatMessage;
using Vention.Application.Chats.Contracts;
using Vention.Application.Chats.Queries.GetChatMessageById;
using Vention.Application.Chats.Queries.GetChatMessagesBySession;
using Vention.Application.Messaging;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("api/chat-sessions/{sessionId:guid}/messages")]
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
            var command = new SendChatMessageCommand(sessionId, request.SenderId, request.Content);
            var result = await _dispatcher.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { sessionId, id = result.Id }, result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ChatMessageResponse>>> GetBySession(
            Guid sessionId,
            CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetChatMessagesBySessionQuery(sessionId), ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ChatMessageResponse>> GetById(
            Guid sessionId,
            Guid id,
            CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetChatMessageByIdQuery(id), ct);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            Guid sessionId,
            Guid id,
            CancellationToken ct)
        {
            await _dispatcher.Send(new DeleteChatMessageCommand(id), ct);
            return NoContent();
        }
    }

    public sealed record SendChatMessageRequest(Guid SenderId, string Content);
}
