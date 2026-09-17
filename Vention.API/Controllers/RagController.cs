using Microsoft.AspNetCore.Mvc;
using Vention.API.Authorization;
using Vention.Application.Abstractions.Auth;
using Vention.Application.Messaging;
using Vention.Application.Rag.Commands.AskQuestion;
using Vention.Application.Rag.Contracts;
using Vention.Domain.Membership;
using Vention.Presentation.Common.Extensions;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("rag")]
    public sealed class RagController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        private readonly ICurrentUserService _currentUser;

        public RagController(IDispatcher dispatcher, ICurrentUserService currentUser)
        {
            _dispatcher = dispatcher;
            _currentUser = currentUser;
        }

        [HttpPost("ask")]
        [RequireOrgRoleFromHeader(
            MembershipRole.Owner,
            MembershipRole.Admin,
            MembershipRole.Editor,
            MembershipRole.Member,
            MembershipRole.Viewer)]
        public async Task<ActionResult<AskQuestionResponse>> Ask(
            [FromBody] AskQuestionRequest request,
            CancellationToken ct)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Question))
                return BadRequest(new { message = "question is required." });

            var organizationId = Request.GetRequiredOrganizationId();

            var result = await _dispatcher.Send(
                new AskQuestionCommand(
                    organizationId,
                    _currentUser.UserId,
                    request.Question,
                    request.FileId),
                ct);

            return Ok(result);
        }
    }

    public sealed record AskQuestionRequest(string Question, Guid? FileId = null);
}