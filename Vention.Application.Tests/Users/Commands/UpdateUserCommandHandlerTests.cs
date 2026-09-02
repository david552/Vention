using Moq;
using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Exceptions;
using Vention.Application.Tests.Users.Common;
using Vention.Application.Users.Commands.UpdateUser;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;
using DomainMembership = Vention.Domain.Membership.Membership;

namespace Vention.Application.Tests.Users.Commands
{

    public sealed class UpdateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IMembershipRepository> _membershipRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        public UpdateUserCommandHandlerTests()
        {
            MapsterTestConfig.EnsureConfigured();
        }

        [Fact]
        public async Task Handle_updates_profile_when_user_updates_themselves()
        {
            var user = UserTestFactory.Create(name: "Old Name");
            var userId = user.Id.Value;

            _userRepository
                .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _unitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new UpdateUserCommand(userId, "New Name", userId),
                CancellationToken.None);

            Assert.Equal("New Name", result.Name);
            Assert.Equal("New Name", user.Name);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_updates_profile_when_acting_user_is_org_admin()
        {
            var organizationId = Guid.NewGuid();
            var actingUserId = Guid.NewGuid();
            var targetUser = UserTestFactory.Create(email: "target@example.com", name: "Target User");

            var targetMembership = DomainMembership.Create(
                targetUser.Id,
                new OrganizationId(organizationId),
                MembershipRole.Member);

            var actingMembership = DomainMembership.Create(
                new UserId(actingUserId),
                new OrganizationId(organizationId),
                MembershipRole.Admin);

            _membershipRepository
                .Setup(x => x.GetByUserIdAsync(targetUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DomainMembership> { targetMembership });

            _membershipRepository
                .Setup(x => x.GetByUserAndOrganizationAsync(
                    new UserId(actingUserId),
                    new OrganizationId(organizationId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(actingMembership);

            _userRepository
                .Setup(x => x.GetByIdAsync(targetUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(targetUser);

            _unitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new UpdateUserCommand(targetUser.Id.Value, "Updated By Admin", actingUserId),
                CancellationToken.None);

            Assert.Equal("Updated By Admin", result.Name);
        }

        [Fact]
        public async Task Handle_throws_not_found_when_user_does_not_exist()
        {
            var missingUserId = Guid.NewGuid();

            _userRepository
                .Setup(x => x.GetByIdAsync(new UserId(missingUserId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var handler = CreateHandler();

            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(
                    new UpdateUserCommand(missingUserId, "New Name", missingUserId),
                    CancellationToken.None));
        }

        [Fact]
        public async Task Handle_throws_forbidden_when_acting_user_cannot_manage_target()
        {
            var organizationId = Guid.NewGuid();
            var actingUserId = Guid.NewGuid();
            var targetUser = UserTestFactory.Create(email: "target@example.com");

            var targetMembership = DomainMembership.Create(
                targetUser.Id,
                new OrganizationId(organizationId),
                MembershipRole.Member);

            _membershipRepository
                .Setup(x => x.GetByUserIdAsync(targetUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DomainMembership> { targetMembership });

            _membershipRepository
                .Setup(x => x.GetByUserAndOrganizationAsync(
                    new UserId(actingUserId),
                    new OrganizationId(organizationId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((DomainMembership?)null);

            var handler = CreateHandler();

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                handler.Handle(
                    new UpdateUserCommand(targetUser.Id.Value, "Blocked Update", actingUserId),
                    CancellationToken.None));
        }

        private UpdateUserCommandHandler CreateHandler()
            => new(
                _userRepository.Object,
                _unitOfWork.Object,
                _membershipRepository.Object,
                new UserAuthorizationService(_membershipRepository.Object));
    }
}