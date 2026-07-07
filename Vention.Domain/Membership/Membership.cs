using Vention.Domain.Common;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Domain.Membership
{
    public sealed class Membership : AggregateRoot<MembershipId>
    {
        public UserId UserId { get; private set; }
        public OrganizationId OrganizationId { get; private set; }
        public MembershipRole Role { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private Membership() { }

        private Membership(MembershipId id, UserId userId, OrganizationId organizationId, MembershipRole role) : base(id)
        {
            UserId = userId;
            OrganizationId = organizationId;
            Role = role;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public static Membership Create(UserId userId, OrganizationId organizationId, MembershipRole role)
        {
            if (userId.Value == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            if (organizationId.Value == Guid.Empty)
                throw new ArgumentException("OrganizationId cannot be empty.", nameof(organizationId));

            return new Membership(new MembershipId(Guid.NewGuid()), userId, organizationId, role);
        }

        public void ChangeRole(MembershipRole newRole)
        {
            Role = newRole;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
    public record MembershipId(Guid Value);
}
