using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CH.CleanArchitecture.Infrastructure.Migrations.Identity
{
    /// <inheritdoc />
    public partial class AddedAreaFlatNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                schema: "Identity",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlatNumber",
                schema: "Identity",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                schema: "Identity",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "FlatNumber",
                schema: "Identity",
                table: "UserAddresses");
        }
    }
}
