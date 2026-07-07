using Microsoft.AspNetCore.Mvc;
using Vention.Application.Messaging;
using Vention.Application.Organizations.Commands.CreateOrganization;
using Vention.Application.Organizations.Commands.DeleteOrganization;
using Vention.Application.Organizations.Commands.UpdateOrganization;
using Vention.Application.Organizations.Contracts;
using Vention.Application.Organizations.Queries.GetOrganizationById;
using Vention.Application.Organizations.Queries.GetOrganizations;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("api/organizations")]
    public sealed class OrganizationsController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        public OrganizationsController(IDispatcher dispatcher) => _dispatcher = dispatcher;

        [HttpPost]
        public async Task<ActionResult<OrganizationResponse>> Create(CreateOrganizationCommand command, CancellationToken ct)
        {
            var result = await _dispatcher.Send(command, ct);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrganizationResponse>> GetById(Guid id, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetOrganizationByIdQuery(id), ct);

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrganizationResponse>>> GetAll(CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetOrganizationsQuery(), ct);

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<OrganizationResponse>> Update(Guid id, [FromBody] UpdateOrganizationRequest request, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new UpdateOrganizationCommand(id, request.Name), ct);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _dispatcher.Send(new DeleteOrganizationCommand(id), ct);

            return NoContent();
        }
    }

    public sealed record UpdateOrganizationRequest(string Name);
}
