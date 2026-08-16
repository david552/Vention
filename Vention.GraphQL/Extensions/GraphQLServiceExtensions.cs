using Vention.GraphQL.DataLoaders;
using Vention.GraphQL.ErrorHandling;
using Vention.GraphQL.Mutations;
using Vention.GraphQL.Queries;
using Vention.GraphQL.Types;

namespace Vention.GraphQL.Extensions
{

    public static class GraphQLServiceExtensions
    {
        public static IServiceCollection AddVentionGraphQL(
            this IServiceCollection services,
            IHostEnvironment environment)
        {
            services
                .AddGraphQLServer()
                .AddQueryType<QueryRoot>()
                .AddMutationType<MutationRoot>()
                .AddType<UserType>()
                .AddType<OrganizationType>()
                .AddType<UserOrganizationMembershipType>()
                .AddDataLoader<OrganizationByIdDataLoader>()
                .AddDataLoader<MembershipsByUserIdDataLoader>()
                .AddErrorFilter<GraphQLErrorFilter>()
                .ModifyRequestOptions(options =>
                {
                    options.IncludeExceptionDetails = environment.IsDevelopment();
                });

            return services;
        }
    }
}