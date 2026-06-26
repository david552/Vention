using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vention.Infrastructure.Database
{
    public class VentionDbContext : DbContext
    {
        public VentionDbContext(DbContextOptions<VentionDbContext> options) : base(options) { }
    }
}
