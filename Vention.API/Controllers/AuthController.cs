using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vention.API.Extensions;
using Vention.Application.Auth.Commands.Login;
using Vention.Application.Auth.Contracts;
using Vention.Application.Messaging;
using Vention.Application.Users.Commands.CreateUser;
using Vention.Application.Users.Contracts;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("auth")]
    [AllowAnonymous]
    public sealed class AuthController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;

        public AuthController(IDispatcher dispatcher) => _dispatcher = dispatcher;

        [HttpPost("login")]
        [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
        {
            var result = await _dispatcher.Send(new LoginCommand(request.Email, request.Password), ct);
            return Ok(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
        public async Task<ActionResult<UserResponse>> Register(
          [FromBody] RegisterRequest request,
          CancellationToken ct)
        {
            var result = await _dispatcher.Send(
                new CreateUserCommand(request.Email, request.Name, request.Password), ct);
            return StatusCode(StatusCodes.Status201Created, result);
        }
    }

    public sealed record LoginRequest(string Email, string Password);
    public sealed record RegisterRequest(string Email, string Name, string Password);

}
