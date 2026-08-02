using MassTransit;
using Microsoft.EntityFrameworkCore;
using Vention.Domain.Chats;
using Vention.Domain.Files;
using Vention.Domain.Membership;
using Vention.Domain.Messages;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Infrastructure.Persistence
{
    public class VentionDbContext : DbContext
    {
        public VentionDbContext(DbContextOptions<VentionDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<Membership> Memberships => Set<Membership>();
        public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
        public DbSet<ChatSessionMember> ChatSessionMembers => Set<ChatSessionMember>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(VentionDbContext).Assembly);
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
