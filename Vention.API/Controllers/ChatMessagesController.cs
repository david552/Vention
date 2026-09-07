using Microsoft.AspNetCore.Mvc;
using Vention.Application.Abstractions;
using Vention.Application.Common;
using Vention.Application.Messages.Commands.SendChatMessage;
using Vention.Application.Messages.Contracts;
using Vention.Application.Messages.Queries.GetChatMessagesBySession;
using Vention.Application.Messaging;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("chats/sessions/{sessionId:guid}/messages")]
    public sealed class ChatMessagesController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        private readonly ICurrentUserService _currentUser;

        public ChatMessagesController(IDispatcher dispatcher, ICurrentUserService currentUser)
        {
            _dispatcher = dispatcher;
            _currentUser = currentUser;
        }

        [HttpPost]
        public async Task<ActionResult<ChatMessageResponse>> Send(
            Guid sessionId,
            [FromBody] SendChatMessageRequest request,
            CancellationToken ct)
        {
            var content = !string.IsNullOrWhiteSpace(request.Content)
                ? request.Content
                : request.Question;

            if (string.IsNullOrWhiteSpace(content))
                return BadRequest(new { message = "content or question is required." });

            var command = new SendChatMessageCommand(sessionId, _currentUser.UserId, content);

            var result = await _dispatcher.Send(command, ct);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBySession(
            Guid sessionId,
            [FromQuery] bool paginated = false,
            [FromQuery] bool all = false,
            [FromQuery] string? cursor = null,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var usePagination = paginated && !all;

            var result = await _dispatcher.Send(
                new GetChatMessagesBySessionQuery(sessionId, _currentUser.UserId, usePagination, cursor, pageSize), ct);

            if (result.Paginated)
                return Ok(new CursorPage<ChatMessageResponse>(result.Items, result.NextCursor));

            return Ok(result.Items);
        }

       
    }

    public sealed record SendChatMessageRequest(string? Content, string? Question);
}
