using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vention.Application.Auth.Contracts
{
    public sealed record AuthMembershipDto(
        Guid OrganisationId,
        string OrganisationName,
        string Role);

    public sealed record AuthResponse(
        Guid Id,
        string Email,
        string Name,
        string Role,
        string AccessToken,
        DateTimeOffset AccessTokenExpiresAt,
        IReadOnlyList<AuthMembershipDto> Memberships);
}
