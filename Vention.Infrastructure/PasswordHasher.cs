using Microsoft.Extensions.Options;
using Vention.Application.Abstractions;
using Vention.Application.Options;

namespace Vention.Infrastructure
{
    public sealed class PasswordHasher : IPasswordHasher
    {
        private readonly CryptoSettingsOptions _options;

        public PasswordHasher(IOptions<CryptoSettingsOptions> options)
        {
            _options = options.Value;
        }

        public string Hash(string password)
        {
            string passwordWithPepper = $"{password}{_options.PasswordPepper}";

            return BCrypt.Net.BCrypt.EnhancedHashPassword(passwordWithPepper);
        }

        public bool Verify(string password, string passwordHash)
        {
            string passwordWithPepper = $"{password}{_options.PasswordPepper}";

            return BCrypt.Net.BCrypt.EnhancedVerify(passwordWithPepper, passwordHash);
        }
    }
}
