namespace Vention.Application.Common
{
    public sealed record ListResult<T>(
        IReadOnlyList<T> Items,
        string? NextCursor,
        bool Paginated);
}
