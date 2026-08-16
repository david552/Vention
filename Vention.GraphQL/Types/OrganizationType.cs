using Vention.GraphQL.Http.Models;

namespace Vention.GraphQL.Types
{

    public sealed class OrganizationType : ObjectType<OrganizationDto>
    {
        protected override void Configure(IObjectTypeDescriptor<OrganizationDto> descriptor)
        {
            descriptor.Name("Organization");
            descriptor.BindFieldsExplicitly();

            descriptor.Field(o => o.Id).Type<NonNullType<UuidType>>();
            descriptor.Field(o => o.Name).Type<NonNullType<StringType>>();
            descriptor.Field(o => o.CreatedAt).Type<NonNullType<DateTimeType>>();
            descriptor.Field(o => o.UpdatedAt).Type<NonNullType<DateTimeType>>();
        }
    }
}