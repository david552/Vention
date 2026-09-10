using Moq;
using Vention.Application.Tests.Users.Common;
using Vention.Application.Users;
using Vention.Application.Users.Queries.GetUsers;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;
using DomainMembership = Vention.Domain.Membership.Membership;

namespace Vention.Application.Tests.Queries
{

    public sealed class GetUsersQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IMembershipRepository> _membershipRepository = new();

        public GetUsersQueryHandlerTests()
        {
            MapsterTestConfig.EnsureConfigured();

            _userRepository
                .Setup(x => x.GetOrphanUsersCreatedByAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User>());
        }

        [Fact]
        public async Task Handle_returns_only_acting_user_when_they_have_no_memberships()
        {
            var actingUser = UserTestFactory.Create(email: "solo@example.com", name: "Solo User");
            var actingUserId = actingUser.Id.Value;

            _membershipRepository
                .Setup(x => x.GetByUserIdAsync(actingUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DomainMembership>());

            _userRepository
                .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<UserId>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { actingUser });

            var handler = CreateHandler();

            var result = await handler.Handle(
                new GetUsersQuery(actingUserId, IncludeOrganisations: false),
                CancellationToken.None);

            var user = Assert.Single(result);
            Assert.Equal(actingUserId, user.Id);
            Assert.Equal("solo@example.com", user.Email);
        }

        [Fact]
        public async Task Handle_returns_users_from_shared_organizations()
        {
            var organizationId = Guid.NewGuid();
            var actingUser = UserTestFactory.Create(email: "acting@example.com", name: "Acting User");
            var teammate = UserTestFactory.Create(email: "teammate@example.com", name: "Teammate");

            var actingMembership = DomainMembership.Create(
                actingUser.Id,
                new OrganizationId(organizationId),
                MembershipRole.Member);

            var teammateMembership = DomainMembership.Create(
                teammate.Id,
                new OrganizationId(organizationId),
                MembershipRole.Member);

            _membershipRepository
                .Setup(x => x.GetByUserIdAsync(actingUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DomainMembership> { actingMembership });

            _membershipRepository
                .Setup(x => x.GetByOrganizationIdAsync(
                    new OrganizationId(organizationId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DomainMembership> { actingMembership, teammateMembership });

            _userRepository
                .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<UserId>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { actingUser, teammate });

            var handler = CreateHandler();

            var result = await handler.Handle(
                new GetUsersQuery(actingUser.Id.Value, IncludeOrganisations: false),
                CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Email == "acting@example.com");
            Assert.Contains(result, x => x.Email == "teammate@example.com");
        }

        [Fact]
        public async Task Handle_admin_sees_only_orphans_they_created()
        {
            var admin = UserTestFactory.Create(email: "admin@example.com", name: "Admin");
            var myOrphan = User.Create(
                Email.Create("orphan@example.com"), "My Orphan", "hash",
                admin.Id);
            var orgId = Guid.NewGuid();

            var adminMembership = DomainMembership.Create(
                admin.Id, new OrganizationId(orgId), MembershipRole.Admin);

            _membershipRepository
                .Setup(x => x.GetByUserIdAsync(admin.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DomainMembership> { adminMembership });

            _membershipRepository
                .Setup(x => x.GetByOrganizationIdAsync(new OrganizationId(orgId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DomainMembership> { adminMembership });

            _userRepository
                .Setup(x => x.GetOrphanUsersCreatedByAsync(admin.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { myOrphan });

            _userRepository
                .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<UserId>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { admin, myOrphan });

            var handler = CreateHandler();
            var result = await handler.Handle(
                new GetUsersQuery(admin.Id.Value, IncludeOrganisations: false),
                CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Email == "orphan@example.com");
        }

        private GetUsersQueryHandler CreateHandler()
        {
            var composer = new UserResponseComposer(
                _membershipRepository.Object,
                Mock.Of<IOrganizationRepository>());

            return new GetUsersQueryHandler(
                _userRepository.Object,
                _membershipRepository.Object,
                composer);
        }
    }
}