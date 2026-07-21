using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vention.Application.Abstractions;
using Vention.Domain.Chats;
using Vention.Domain.Membership;
using Vention.Domain.Messages;
using Vention.Domain.Organizations;
using Vention.Domain.Users;
using Vention.Infrastructure.Persistence;

namespace Vention.Infrastructure.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(VentionDbContext context, IPasswordHasher passwordHasher)
        {
            if (await context.Set<User>().AnyAsync())
                return;
            var adminUser = User.Create(
                Email.Create("admin@vention.com"),
                "Admin User",
                passwordHasher.Hash("Admin1234!"));

            var regularUser = User.Create(
                Email.Create("david.piranishvili@vention.com"),
                "David Piranishvili",
                passwordHasher.Hash("User1234!"));

            context.Set<User>().AddRange(adminUser, regularUser);

            //organisation
            var ventionOrg = Organization.Create("Vention Labs");
            context.Set<Organization>().Add(ventionOrg);

            var adminMembership = Membership.Create(adminUser.Id, ventionOrg.Id, MembershipRole.Admin);
            var regularMembership = Membership.Create(regularUser.Id, ventionOrg.Id, MembershipRole.Member);
            context.Set<Membership>().AddRange(adminMembership, regularMembership);

            //char session
            var generalChat = ChatSession.CreateDirectChat(ventionOrg.Id, adminUser.Id, regularUser.Id);
            context.Set<ChatSession>().Add(generalChat);

            var adminMember = ChatSessionMember.Create(generalChat.Id, adminUser.Id);
            var regularMember = ChatSessionMember.Create(generalChat.Id, regularUser.Id);
            context.Set<ChatSessionMember>().AddRange(adminMember, regularMember);

            //chat messages
            var message1 = ChatMessage.Create(generalChat.Id, adminUser.Id, "Hello!");
            var message2 = ChatMessage.Create(generalChat.Id, regularUser.Id, "Hi!");
            context.Set<ChatMessage>().AddRange(message1, message2);

            await context.SaveChangesAsync();
        }
    }
}
