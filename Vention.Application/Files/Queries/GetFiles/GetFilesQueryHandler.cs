using Mapster;
using Vention.Application.Files.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Files;
using Vention.Domain.Organizations;

namespace Vention.Application.Files.Queries.GetFiles
{
    public sealed class GetFilesQueryHandler : IQueryHandler<GetFilesQuery, IReadOnlyList<FileResponse>>
    {
        private readonly IStoredFileRepository _storedFileRepository;

        public GetFilesQueryHandler(IStoredFileRepository storedFileRepository)
            => _storedFileRepository = storedFileRepository;

        public async Task<IReadOnlyList<FileResponse>> Handle(GetFilesQuery query, CancellationToken ct)
        {
            var files = await _storedFileRepository.GetByOrganizationAsync(
                new OrganizationId(query.OrganizationId),
                query.Limit,
                ct);

            return files.Adapt<IReadOnlyList<FileResponse>>();
        }
    }
}