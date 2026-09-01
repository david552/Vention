using GreenDonut;
using Vention.GraphQL.Http.Clients;
using Vention.GraphQL.Http.Models;

namespace Vention.GraphQL.DataLoaders
{

    public sealed class OrganizationByIdDataLoader : BatchDataLoader<Guid, OrganizationDto>
    {
        private readonly IVentionApiClient _api;

        public OrganizationByIdDataLoader(
            IVentionApiClient api,
            IBatchScheduler batchScheduler,
            DataLoaderOptions options)
            : base(batchScheduler, options)
            => _api = api;

        protected override async Task<IReadOnlyDictionary<Guid, OrganizationDto>> LoadBatchAsync(
            IReadOnlyList<Guid> keys,
            CancellationToken cancellationToken)
        {
            var organizations = await _api.GetOrganizationsByIdsAsync(keys, cancellationToken);
            return organizations.ToDictionary(o => o.Id);
        }
    }
}