using Vention.Domain.Common;

namespace Vention.Domain.Organizations
{
    public sealed class Organization : AggregateRoot<OrganizationId>
    {
        public string Name { get; private set; } = null!;
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTimeOffset? DeletedAt { get; private set; }

        private Organization() { } 

        private Organization(OrganizationId id, string name) : base(id)
        {
            Name = name;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public static Organization Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));

            return new Organization(new OrganizationId(Guid.NewGuid()), name.Trim());
        }

        public void Rename(string name)
        {
            EnsureNotDeleted();

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));

            Name = name.Trim();
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Delete()
        {
            if (IsDeleted)
                throw new InvalidOperationException("Organization is already deleted.");

            IsDeleted = true;
            DeletedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DeletedAt.Value;
        }

        private void EnsureNotDeleted()
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot modify a deleted organization.");
        }
    }


    public record OrganizationId(Guid Value);
}
