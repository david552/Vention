using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vention.API.Protos;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Application.Users.Queries.GetUserById;

namespace Vention.API.GrpcServices
{
    [AllowAnonymous]
    public sealed class UserGrpcService : UserService.UserServiceBase
    {
        private readonly IDispatcher _dispatcher;
        public UserGrpcService(IDispatcher dispatcher) => _dispatcher = dispatcher;
        public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.UserId, out var userId) ||
                !Guid.TryParse(request.ActingUserId, out var actingUserId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "user_id and acting_user_id must be valid GUIDs."));
            }

            var user = await _dispatcher.Send(new GetUserByIdQuery(userId, actingUserId), context.CancellationToken);

            var response = new GetUserByIdResponse
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Name = user.Name,
                CreatedAt = user.CreatedAt.ToString("O"),
                UpdatedAt = user.UpdatedAt.ToString("O")
            };

            foreach (var org in user.Organisations)
            {
                response.Organisations.Add(new OrganizationMembership
                {
                    Id = org.Id.ToString(),
                    Name = org.Name,
                    Role = org.Role
                });
            }

            return response;

        }
    }
}
