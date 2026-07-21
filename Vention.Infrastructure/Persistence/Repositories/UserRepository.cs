using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vention.Domain.Users;

namespace Vention.Infrastructure.Persistence.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly VentionDbContext _context;
        public UserRepository(VentionDbContext context) => _context = context;

        public Task<User?> GetByIdAsync(UserId id, CancellationToken ct)
            => _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        public Task<User?> GetByEmailAsync(Email email, CancellationToken ct)
            => _context.Users.FirstOrDefaultAsync(u => u.Email.Value == email.Value, ct);

        public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct)
            => await _context.Users.AsNoTracking().ToListAsync(ct);

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken ct)
            => _context.Users.AnyAsync(u => u.Email.Value == email.Value, ct);

        public Task<bool> ExistsByIdAsync(UserId id, CancellationToken ct)
            => _context.Users.AnyAsync(u => u.Id == id, ct);

        public void Add(User user) => _context.Users.Add(user);

        public async Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyCollection<UserId> ids, CancellationToken ct)
        {
            if (ids.Count == 0)
                return Array.Empty<User>();


            return await _context.Users
                .AsNoTracking()
                .Where(u => ids.Contains(u.Id))
                .ToListAsync(ct);
        }
    }
}
