using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CH.CleanArchitecture.Infrastructure.Migrations.Application
{
    /// <inheritdoc />
    public partial class EnhancedNotificationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "UserFor",
                schema: "App",
                table: "Notifications",
                newName: "RecipientPhone");

            migrationBuilder.AddColumn<string>(
                name: "RecipientEmail",
                schema: "App",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientId",
                schema: "App",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "RecipientEmail",
                schema: "App",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RecipientId",
                schema: "App",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "RecipientPhone",
                schema: "App",
                table: "Notifications",
                newName: "UserFor");
        }
    }
}
