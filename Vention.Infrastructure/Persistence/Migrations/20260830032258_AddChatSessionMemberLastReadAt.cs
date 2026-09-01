using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vention.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChatSessionMemberLastReadAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_read_at",
                table: "chat_session_members",
                type: "timestamptz",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_read_at",
                table: "chat_session_members");
        }
    }
}
