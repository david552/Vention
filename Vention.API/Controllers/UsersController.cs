using Microsoft.AspNetCore.Mvc;
using Vention.Application.Messaging;
using Vention.Application.Users.Commands.CreateUser;
using Vention.Application.Users.Commands.DeleteUser;
using Vention.Application.Users.Commands.UpdateUser;
using Vention.Application.Users.Contracts;
using Vention.Application.Users.Queries.GetUserById;
using Vention.Application.Users.Queries.GetUsers;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        public UsersController(IDispatcher dispatcher) => _dispatcher = dispatcher;

        [HttpPost]
        public async Task<ActionResult<UserResponse>> Create(CreateUserCommand command, CancellationToken ct)
        {
            var result = await _dispatcher.Send(command, ct);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetUserByIdQuery(id), ct);

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetUsersQuery(), ct);

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new UpdateUserCommand(id, request.Name), ct);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _dispatcher.Send(new DeleteUserCommand(id), ct);

            return NoContent();
        }
    }

    public sealed record UpdateUserRequest(string Name);
}
