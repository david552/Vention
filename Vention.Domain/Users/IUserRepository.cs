namespace Vention.Domain.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(UserId id, CancellationToken ct);
        Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyCollection<UserId> ids, CancellationToken ct);
        Task<User?> GetByEmailAsync(Email email, CancellationToken ct);
        Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct);
        Task<bool> ExistsByEmailAsync(Email email, CancellationToken ct);
        Task<bool> ExistsByIdAsync(UserId id, CancellationToken ct);

        Task<IReadOnlyList<User>> GetUsersWithNoMembershipsAsync(CancellationToken ct);

        void Add(User user);
    }
}
