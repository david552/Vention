using Mapster;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using Vention.Application.User.Requests;
using Vention.Application.User.Responses;


namespace Vention.Application.User.Services
{
    public class UserService : IUserService
    {
        private static readonly List<Domain.User> _users = new();
        private readonly CryptoSettingsOptions _cryptoSettings;

        public UserService(IOptions<CryptoSettingsOptions> cryptoSettings)
        {
            _cryptoSettings = cryptoSettings.Value;
        }

        public UserResponseModel Create(UserRequestCreateModel model)
        {
            var user = model.Adapt<Domain.User>();

            user.Id = _users.Count + 1;

            string passwordWithPepper = model.Password + _cryptoSettings.PasswordPepper;
            user.PasswordHash = HashPassword(passwordWithPepper);

            _users.Add(user);

            return user.Adapt<UserResponseModel>();
        }

        public IEnumerable<UserResponseModel> GetAll()
        {
            return _users.Adapt<IEnumerable<UserResponseModel>>();
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
