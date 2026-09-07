using Moq;
using Vention.Application.Authorization;
using Vention.Application.Exceptions;
using Vention.Application.Tests.Users.Common;
using Vention.Application.Users;
using Vention.Application.Users.Queries.GetUserById;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

using DomainMembership = Vention.Domain.Membership.Membership;

namespace Vention.Application.Tests.Queries
{

    public sealed class GetUserByIdQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IMembershipRepository> _membershipRepository = new();

        public GetUserByIdQueryHandlerTests()
        {
            MapsterTestConfig.EnsureConfigured();
        }

        [Fact]
        public async Task Handle_returns_user_when_acting_user_views_themselves()
        {
            var user = UserTestFactory.Create(email: "self@example.com", name: "Self User");
            var userId = user.Id.Value;

            _userRepository
                .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new GetUserByIdQuery(userId, userId, IncludeOrganisations: false),
                CancellationToken.None);

            Assert.Equal(userId, result.Id);
            Assert.Equal("self@example.com", result.Email);
            Assert.Equal("Self User", result.Name);
            Assert.Empty(result.Organisations);
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
                    new GetUserByIdQuery(missingUserId, missingUserId, IncludeOrganisations: false),
                    CancellationToken.None));
        }

        [Fact]
        public async Task Handle_throws_forbidden_when_acting_user_has_no_memberships()
        {
            var actingUserId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();

            _membershipRepository
                .Setup(x => x.GetByUserIdAsync(new UserId(actingUserId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DomainMembership>());

            var handler = CreateHandler();

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                handler.Handle(
                    new GetUserByIdQuery(targetUserId, actingUserId, IncludeOrganisations: false),
                    CancellationToken.None));
        }

        private GetUserByIdQueryHandler CreateHandler()
        {
            var composer = new UserResponseComposer(
                _membershipRepository.Object,
                Mock.Of<IOrganizationRepository>());

            return new GetUserByIdQueryHandler(
                _userRepository.Object,
                new UserAuthorizationService(_membershipRepository.Object),
                composer);
        }
    }
}