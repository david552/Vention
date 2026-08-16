using Vention.GraphQL.DataLoaders;
using Vention.GraphQL.Http.Models;

namespace Vention.GraphQL.Types
{

    public sealed class UserType : ObjectType<UserDto>
    {
        protected override void Configure(IObjectTypeDescriptor<UserDto> descriptor)
        {
            descriptor.Name("User");
            descriptor.BindFieldsExplicitly();

            descriptor.Field(u => u.Id).Type<NonNullType<UuidType>>();
            descriptor.Field(u => u.Email).Type<NonNullType<StringType>>();
            descriptor.Field(u => u.Name).Type<NonNullType<StringType>>();
            descriptor.Field(u => u.CreatedAt).Type<NonNullType<DateTimeType>>();
            descriptor.Field(u => u.UpdatedAt).Type<NonNullType<DateTimeType>>();

            descriptor
                .Field("organisations")
                .Type<NonNullType<ListType<NonNullType<UserOrganizationMembershipType>>>>()
                .ResolveWith<UserOrganisationsResolver>(r =>
                    r.GetOrganisationsAsync(default!, default!, default!, default!));
        }
    }

    internal sealed class UserOrganisationsResolver
    {
        public async Task<IReadOnlyList<UserOrganizationMembershipDto>> GetOrganisationsAsync(
            [Parent] UserDto user,
            MembershipsByUserIdDataLoader membershipsByUserId,
            OrganizationByIdDataLoader organizationsById,
            CancellationToken ct)
        {
            var memberships = await membershipsByUserId.LoadAsync(user.Id, ct);

            if (memberships == null || !memberships.Any())
                return Array.Empty<UserOrganizationMembershipDto>();

            var orgIds = memberships.Select(m => m.OrganisationId).Distinct().ToArray();
            var orgs = await organizationsById.LoadAsync(orgIds, ct);

            var result = new List<UserOrganizationMembershipDto>();

            foreach (var membership in memberships)
            {
                var org = orgs.FirstOrDefault(o => o != null && o.Id == membership.OrganisationId);

                if (org == null) continue;

                result.Add(new UserOrganizationMembershipDto
                {
                    Id = org.Id,
                    Name = org.Name,
                    Role = membership.Type
                });
            }
            return result;
        }
    }
}