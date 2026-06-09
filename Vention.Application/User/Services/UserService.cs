using Microsoft.Extensions.Options;
using System;
using System.Text;


namespace Vention.Application.User.Services
{
    public class UserService : IUserService
    {
        private readonly CryptoSettingsOptions _cryptoSettings;

        public UserService(IOptions<CryptoSettingsOptions> cryptoSettings)
        {
            _cryptoSettings = cryptoSettings.Value;
        }


        public string HashPassword(string password)
        {
            string passwordWithPepper = password + _cryptoSettings.PasswordPepper;

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
