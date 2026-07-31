using Microsoft.AspNetCore.Mvc;
using Vention.API.Authorization;
using Vention.Application.Abstractions;
using Vention.Application.Messaging;
using Vention.Application.Organizations.Commands.CreateOrganization;
using Vention.Application.Organizations.Commands.DeleteOrganization;
using Vention.Application.Organizations.Commands.UpdateOrganization;
using Vention.Application.Organizations.Contracts;
using Vention.Application.Organizations.Queries.GetOrganizationById;
using Vention.Application.Organizations.Queries.GetOrganizations;
using Vention.Domain.Membership;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("orgs")]
    public sealed class OrganizationsController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        private readonly ICurrentUserService _currentUser;

        public OrganizationsController(IDispatcher dispatcher, ICurrentUserService currentUser)
        {
            _dispatcher = dispatcher;
            _currentUser = currentUser;
        }

        [HttpPost]
        public async Task<ActionResult<OrganizationResponse>> Create([FromBody] CreateOrganizationRequest request, CancellationToken ct)
        {

            var command = new CreateOrganizationCommand(request.Name, _currentUser.UserId);
            var result = await _dispatcher.Send(command, ct);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        [RequireOrganizationRole("id", MembershipRole.Owner, MembershipRole.Admin, MembershipRole.Member)]
        public async Task<ActionResult<OrganizationResponse>> GetById(Guid id, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetOrganizationByIdQuery(id), ct);

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrganizationResponse>>> GetAll(CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetOrganizationsQuery(_currentUser.UserId), ct);

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [HttpPatch("{id:guid}")]
        [RequireOrganizationRole("id", MembershipRole.Owner, MembershipRole.Admin)] 
        public async Task<ActionResult<OrganizationResponse>> Update(Guid id, [FromBody] UpdateOrganizationRequest request, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new UpdateOrganizationCommand(id, request.Name), ct);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [RequireOrganizationRole("id", MembershipRole.Owner)] 
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _dispatcher.Send(new DeleteOrganizationCommand(id), ct);

            return NoContent();
        }
    }

    public sealed record CreateOrganizationRequest(string Name);
    public sealed record UpdateOrganizationRequest(string Name);
}
