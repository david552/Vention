namespace Vention.Application.Membership.Contracts
{
    public sealed record MembershipResponse(Guid Id, Guid UserId, Guid OrganizationId, string Role);

}
