using Microsoft.AspNetCore.Mvc;
using Vention.Application.Membership.Commands.ChangeMembershipRole;
using Vention.Application.Membership.Commands.CreateMembership;
using Vention.Application.Membership.Commands.DeleteMembership;
using Vention.Application.Membership.Contracts;
using Vention.Application.Membership.Queries.GetMembershipById;
using Vention.Application.Membership.Queries.GetMembershipsByOrganization;
using Vention.Application.Membership.Queries.GetMembershipsByUser;
using Vention.Application.Messaging;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("api/memberships")]
    public sealed class MembershipsController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        public MembershipsController(IDispatcher dispatcher) => _dispatcher = dispatcher;

        [HttpPost]
        public async Task<ActionResult<MembershipResponse>> Create(CreateMembershipCommand command, CancellationToken ct)
        {
            var result = await _dispatcher.Send(command, ct);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MembershipResponse>> GetById(Guid id, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetMembershipByIdQuery(id), ct);

            return Ok(result);
        }

        [HttpGet("by-organization/{organizationId:guid}")]
        public async Task<ActionResult<IReadOnlyList<MembershipResponse>>> GetByOrganization(Guid organizationId, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetMembershipsByOrganizationQuery(organizationId), ct);

            return Ok(result);
        }

        [HttpGet("by-user/{userId:guid}")]
        public async Task<ActionResult<IReadOnlyList<MembershipResponse>>> GetByUser(Guid userId, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetMembershipsByUserQuery(userId), ct);

            return Ok(result);
        }

        [HttpPut("{id:guid}/role")]
        public async Task<ActionResult<MembershipResponse>> ChangeRole(Guid id, [FromBody] ChangeMembershipRoleRequest request, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new ChangeMembershipRoleCommand(id, request.Role), ct);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _dispatcher.Send(new DeleteMembershipCommand(id), ct);

            return NoContent();
        }
    }

    public sealed record ChangeMembershipRoleRequest(string Role);
}
