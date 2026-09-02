using Moq;
using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Exceptions;
using Vention.Application.Tests.Users.Common;
using Vention.Application.Users.Commands.DeleteUser;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;
using DomainMembership = Vention.Domain.Membership.Membership;


namespace Vention.Application.Tests.Users.Commands
{

    public sealed class DeleteUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IMembershipRepository> _membershipRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        [Fact]
        public async Task Handle_soft_deletes_user_and_removes_memberships()
        {
            var user = UserTestFactory.Create();
            var userId = user.Id.Value;

            var membership = DomainMembership.Create(
                user.Id,
                new OrganizationId(Guid.NewGuid()),
                MembershipRole.Member);

            _userRepository
                .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _membershipRepository
                .Setup(x => x.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DomainMembership> { membership });

            _unitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = CreateHandler();

            await handler.Handle(
                new DeleteUserCommand(userId, userId),
                CancellationToken.None);

            Assert.True(user.IsDeleted);
            _membershipRepository.Verify(x => x.Remove(membership), Times.Once);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
                    new DeleteUserCommand(missingUserId, missingUserId),
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
                    new DeleteUserCommand(targetUser.Id.Value, actingUserId),
                    CancellationToken.None));
        }

        private DeleteUserCommandHandler CreateHandler()
            => new(
                _userRepository.Object,
                _membershipRepository.Object,
                _unitOfWork.Object,
                new UserAuthorizationService(_membershipRepository.Object));
    }
}