using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Vention.Application.User.Requests;
using Vention.Application.User.Responses;
using Vention.Application.User.Services;

namespace Vention.Application.Tests
{
    public class UserServiceTest
    {
        private readonly UserService _service;
        private readonly Mock<IOptions<CryptoSettingsOptions>> _cryptoSettingsMock;
        private const string TestPepper = "Secret";

        public UserServiceTest()
        {
            _cryptoSettingsMock = new Mock<IOptions<CryptoSettingsOptions>>();

            _cryptoSettingsMock
                .Setup(x => x.Value)
                .Returns(new CryptoSettingsOptions { PasswordPepper = TestPepper });

            _service = new UserService(_cryptoSettingsMock.Object);
        }

        #region Create Tests

        [Fact]
        public void Create_ShouldReturnUserResponseModel_WhenModelIsValid()
        {
            var model = new UserRequestCreateModel
            {
                Name = "David",
                LastName = "Piranishvili",
                UserName = "david552",
                Password = "SecurePassword"
            };

            var result = _service.Create(model);

            Assert.NotNull(result);
            Assert.Equal(model.Name, result.Name);
            Assert.Equal(model.LastName, result.LastName);
            Assert.Equal(model.UserName, result.UserName);
            Assert.True(result.Id > 0);

            _cryptoSettingsMock.Verify(x => x.Value, Times.Once);
        }

        #endregion

        #region GetAll Tests

        [Fact]
        public void GetAll_ShouldReturnUserResponseModels_WhenCalled()
        {
            var model = new UserRequestCreateModel
            {
                Name = "David",
                LastName = "Piranishvili",
                UserName = "david552",
                Password = "SecurePassword"
            };
            _service.Create(model);

            var result = _service.GetAll();

            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<UserResponseModel>>(result);
            Assert.Contains(result, u => u.UserName == "david552");
        }

        #endregion
    }
}