using GreenDonut;
using Vention.GraphQL.Http.Clients;
using Vention.GraphQL.Http.Models;

namespace Vention.GraphQL.DataLoaders
{

    public sealed class MembershipsByUserIdDataLoader : GroupedDataLoader<Guid, MembershipDto>
    {
        private readonly IVentionApiClient _api;

        public MembershipsByUserIdDataLoader(
            IVentionApiClient api,
            IBatchScheduler batchScheduler,
            DataLoaderOptions options)
            : base(batchScheduler, options)
            => _api = api;

        protected override async Task<ILookup<Guid, MembershipDto>> LoadGroupedBatchAsync(
            IReadOnlyList<Guid> keys,
            CancellationToken cancellationToken)
        {
            var memberships = await _api.GetMembershipsByUserIdsAsync(keys, cancellationToken);
            return memberships.ToLookup(m => m.UserId);
        }
    }
}