using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vention.Application.Abstractions;
using Vention.Infrastructure.Persistence;

namespace Vention.Infrastructure
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly VentionDbContext _context;
        public UnitOfWork(VentionDbContext context) => _context = context;

        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
    }
}
