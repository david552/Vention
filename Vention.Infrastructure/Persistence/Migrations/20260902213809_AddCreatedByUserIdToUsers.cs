using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vention.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUserIdToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_created_by_user_id",
                table: "users",
                column: "created_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_users_created_by_user_id",
                table: "users",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_users_created_by_user_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_created_by_user_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "users");
        }
    }
}
