using Moq;

using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Tests.Users.Common;
using Vention.Application.Users.Commands.CreateUser;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

using DomainMembership = Vention.Domain.Membership.Membership;

namespace Vention.Application.Tests.Users.Commands
{
    public sealed class CreateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IPasswordHasher> _passwordHasher = new();
        private readonly Mock<IMembershipRepository> _membershipRepository = new();

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
            var actingUserId = Guid.NewGuid();
            SetupActingUserAsAdmin(actingUserId);

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

            var handler = CreateHandler();

            var result = await handler.Handle(
                new CreateUserCommand("new.user@example.com", "New User", "Password123!", actingUserId),
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
            var actingUserId = Guid.NewGuid();
            SetupActingUserAsAdmin(actingUserId);

            _userRepository
                .Setup(x => x.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = CreateHandler();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(
                    new CreateUserCommand("existing@example.com", "Existing User", "Password123!", actingUserId),
                    CancellationToken.None));

            Assert.Contains("existing@example.com", exception.Message);

            _userRepository.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_throws_forbidden_when_created_by_user_id_is_null()
        {
            var handler = CreateHandler();

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                handler.Handle(
                    new CreateUserCommand("x@example.com", "X", "Password123!", null),
                    CancellationToken.None));
        }

        [Fact]
        public async Task Handle_throws_forbidden_when_acting_user_is_not_owner_or_admin()
        {
            var actingUserId = Guid.NewGuid();

            var memberOnly = DomainMembership.Create(
                new UserId(actingUserId),
                new OrganizationId(Guid.NewGuid()),
                MembershipRole.Member);

            _membershipRepository
                .Setup(x => x.GetByUserIdAsync(new UserId(actingUserId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DomainMembership> { memberOnly });

            var handler = CreateHandler();

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                handler.Handle(
                    new CreateUserCommand("x@example.com", "X", "Password123!", actingUserId),
                    CancellationToken.None));
        }

        private void SetupActingUserAsAdmin(Guid actingUserId)
        {
            var actingMembership = DomainMembership.Create(
                new UserId(actingUserId),
                new OrganizationId(Guid.NewGuid()),
                MembershipRole.Admin);

            _membershipRepository
                .Setup(x => x.GetByUserIdAsync(new UserId(actingUserId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DomainMembership> { actingMembership });
        }

        private CreateUserCommandHandler CreateHandler()
            => new(
                _userRepository.Object,
                _unitOfWork.Object,
                _passwordHasher.Object,
                _membershipRepository.Object);
    }
}