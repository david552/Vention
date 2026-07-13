using Vention.Domain.Common;

namespace Vention.Domain.Users
{
    public sealed class User : AggregateRoot<UserId>
    {
        public Email Email { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTimeOffset? DeletedAt { get; private set; }



        private User() { } 

        private User(UserId id, Email email, string name, string passwordHash) : base(id)
        {
            Email = email;
            Name = name;
            PasswordHash = passwordHash;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            IsDeleted = false;
        }

        public static User Create(Email email, string name, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

            return new User(new UserId(Guid.NewGuid()), email, name.Trim(), passwordHash);
        }

        public void UpdateProfile(string name)
        {
            EnsureNotDeleted();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));

            Name = name.Trim();
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void ChangePassword(string newPasswordHash)
        {
            EnsureNotDeleted();
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Delete()
        {
            if (IsDeleted)
                throw new InvalidOperationException("User is already deleted.");

            IsDeleted = true;
            DeletedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DeletedAt.Value;
        }

        private void EnsureNotDeleted()
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot modify a deleted user.");
        }
    }

    public record UserId(Guid Value);
}
