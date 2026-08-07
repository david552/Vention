using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vention.API.Extensions;
using Vention.Application.Abstractions;
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
    [Route("users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        private readonly ICurrentUserService _currentUser;

        public UsersController(IDispatcher dispatcher, ICurrentUserService currentUser) {
            _dispatcher = dispatcher;
            _currentUser = currentUser;
        }

        [HttpPost]
        [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
        public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
        {
            var result = await _dispatcher.Send(
                new CreateUserCommand(request.Email, request.DisplayName, request.Password), ct);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetUserByIdQuery(id, _currentUser.UserId), ct);

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken ct)
        {
            var result = await _dispatcher.Send(new GetUsersQuery(_currentUser.UserId), ct);

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [HttpPatch("{id:guid}")]
        public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new UpdateUserCommand(id, request.DisplayName, _currentUser.UserId), ct);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _dispatcher.Send(new DeleteUserCommand(id, _currentUser.UserId), ct);

            return NoContent();
        }
    }

    public sealed record UpdateUserRequest(string DisplayName);
    public sealed record CreateUserRequest(string Email, string DisplayName, string Password);

}
