using Microsoft.AspNetCore.Mvc;
using Vention.Application.Abstractions;
using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;
using Vention.Application.Users.Queries.GetAllOnlineUsers;
using Vention.Application.Users.Queries.GetOnlineUsersByOrganization;

namespace Vention.API.Controllers
{

    [ApiController]
    [Route("users/online")]
    public sealed class OnlineUsersController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        private readonly ICurrentUserService _currentUser;

        public OnlineUsersController(IDispatcher dispatcher, ICurrentUserService currentUser)
        {
            _dispatcher = dispatcher;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OnlineUserResponse>>> GetAll(CancellationToken ct)
        {
            var result = await _dispatcher.Send(
                new GetAllOnlineUsersQuery(_currentUser.UserId), ct);

            return Ok(result);
        }

        [HttpGet("organizations/{organizationId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OnlineUserResponse>>> GetByOrganization(
            Guid organizationId,
            CancellationToken ct)
        {
            var result = await _dispatcher.Send(
                new GetOnlineUsersByOrganizationQuery(organizationId, _currentUser.UserId), ct);

            return Ok(result);
        }
    }
}