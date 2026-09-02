using Moq;
using Vention.Application.Abstractions;
using Vention.Application.Tests.Users.Common;
using Vention.Application.Users.Commands.CreateUser;
using Vention.Domain.Users;

namespace Vention.Application.Tests.Users.Commands
{

    public sealed class CreateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IPasswordHasher> _passwordHasher = new();

        public CreateUserCommandHandlerTests()
        {
            MapsterTestConfig.EnsureConfigured();

            _passwordHasher
                .Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("hashed-password");
        }

        [Fact]
        public async Task Handle_creates_user_when_email_is_unique()
        {
            User? addedUser = null;

            _userRepository
                .Setup(x => x.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userRepository
                .Setup(x => x.Add(It.IsAny<User>()))
                .Callback<User>(user => addedUser = user);

            _unitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreateUserCommandHandler(
                _userRepository.Object,
                _unitOfWork.Object,
                _passwordHasher.Object);

            var result = await handler.Handle(
                new CreateUserCommand("new.user@example.com", "New User", "Password123!"),
                CancellationToken.None);

            Assert.NotNull(addedUser);
            Assert.Equal("new.user@example.com", result.Email);
            Assert.Equal("New User", result.Name);
            Assert.Equal(addedUser.Id.Value, result.Id);

            _userRepository.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _passwordHasher.Verify(x => x.Hash("Password123!"), Times.Once);
        }

        [Fact]
        public async Task Handle_throws_when_email_already_exists()
        {
            _userRepository
                .Setup(x => x.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new CreateUserCommandHandler(
                _userRepository.Object,
                _unitOfWork.Object,
                _passwordHasher.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(
                    new CreateUserCommand("existing@example.com", "Existing User", "Password123!"),
                    CancellationToken.None));

            Assert.Contains("existing@example.com", exception.Message);

            _userRepository.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}