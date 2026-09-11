using Vention.Domain.Users;

namespace Vention.Application.Tests.Users.Common
{

    internal static class UserTestFactory
    {
        public static User Create(
            string email = "user@example.com",
            string name = "Test User",
            string passwordHash = "hashed-password")
            => User.Create(Email.Create(email), name, passwordHash);
    }
}