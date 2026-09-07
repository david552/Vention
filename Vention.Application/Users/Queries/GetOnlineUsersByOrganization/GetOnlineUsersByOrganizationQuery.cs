using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;

namespace Vention.Application.Users.Queries.GetOnlineUsersByOrganization
{
    public sealed record GetOnlineUsersByOrganizationQuery(
     Guid OrganizationId,
     Guid ActingUserId) : IQuery<IReadOnlyList<OnlineUserResponse>>;
}
