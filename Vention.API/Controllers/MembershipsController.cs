using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Vention.API.Authorization;
using Vention.API.Extensions;
using Vention.Application.Membership.Commands.ChangeMembershipRole;
using Vention.Application.Membership.Commands.CreateMembership;
using Vention.Application.Membership.Commands.DeleteMembership;
using Vention.Application.Membership.Commands.DeleteMembershipByUserAndOrganization;
using Vention.Application.Membership.Contracts;
using Vention.Application.Membership.Queries.GetMembershipById;
using Vention.Application.Membership.Queries.GetMembershipsByOrganization;
using Vention.Application.Membership.Queries.GetMembershipsByUser;
using Vention.Application.Messaging;
using Vention.Domain.Membership;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("memberships")]
    public sealed class MembershipsController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        public MembershipsController(IDispatcher dispatcher) => _dispatcher = dispatcher;

        [HttpPost]
        public async Task<ActionResult<MembershipResponse>> Create([FromBody] CreateMembershipRequest request, CancellationToken ct)
        {
            var organizationId = request.ResolveOrganizationId();
            var command = new CreateMembershipCommand(request.UserId, organizationId, request.Role, User.GetUserId());
            var result = await _dispatcher.Send(command, ct);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MembershipResponse>> GetById(Guid id, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetMembershipByIdQuery(id, User.GetUserId()), ct);

            return Ok(result);
        }

        [HttpGet("by-organization/{organizationId:guid}")]
        [RequireOrganizationRole("organizationId", MembershipRole.Owner, MembershipRole.Admin, MembershipRole.Member)]
        public async Task<ActionResult<IReadOnlyList<MembershipResponse>>> GetByOrganization(Guid organizationId, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetMembershipsByOrganizationQuery(organizationId), ct);

            return Ok(result);
        }

        [HttpGet("by-user/{userId:guid}")]
        public async Task<ActionResult<IReadOnlyList<MembershipResponse>>> GetByUser(Guid userId, CancellationToken ct)
        {
            var actingUserId = User.GetUserId();

            if (userId != actingUserId)
                return Forbid();

            var result = await _dispatcher.Send(new GetMembershipsByUserQuery(userId), ct);

            return Ok(result);
        }

        [HttpPut("{id:guid}/role")]
        public async Task<ActionResult<MembershipResponse>> ChangeRole(Guid id, [FromBody] ChangeMembershipRoleRequest request, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new ChangeMembershipRoleCommand(id, request.Role, User.GetUserId()), ct);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _dispatcher.Send(new DeleteMembershipCommand(id, User.GetUserId()), ct);

            return NoContent();
        }

        [HttpDelete("{userId:guid}/{organizationId:guid}")]
        public async Task<IActionResult> DeleteByUserAndOrganization(Guid userId, Guid organizationId, CancellationToken ct)
        {
            await _dispatcher.Send(new DeleteMembershipByUserAndOrganizationCommand(userId, organizationId, User.GetUserId()), ct);

            return NoContent();
        }
    }

    public sealed record ChangeMembershipRoleRequest(string Role);

    public sealed class CreateMembershipRequest
    {
        public Guid UserId { get; init; }
        public Guid OrganizationId { get; init; }
        public Guid OrganisationId { get; init; }
        public string Role { get; init; } = string.Empty;
        public Guid ResolveOrganizationId()
        {
            if (OrganisationId != Guid.Empty)
                return OrganisationId;
            if (OrganizationId != Guid.Empty)
                return OrganizationId;
            throw new ArgumentException(
                "organisationId (or organizationId) is required.",
                nameof(OrganisationId));
        }
    }
}
