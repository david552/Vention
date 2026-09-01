using Vention.GraphQL.Http.Models;

namespace Vention.GraphQL.Types
{

    public sealed class UserOrganizationMembershipType : ObjectType<UserOrganizationMembershipDto>
    {
        protected override void Configure(IObjectTypeDescriptor<UserOrganizationMembershipDto> descriptor)
        {
            descriptor.Name("UserOrganizationMembership");
            descriptor.BindFieldsExplicitly();

            descriptor.Field(x => x.Id).Type<NonNullType<UuidType>>();
            descriptor.Field(x => x.Name).Type<NonNullType<StringType>>();
            descriptor.Field(x => x.Role).Type<NonNullType<StringType>>();
        }
    }
}