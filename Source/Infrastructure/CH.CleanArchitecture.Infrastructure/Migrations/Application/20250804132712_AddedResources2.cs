using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CH.CleanArchitecture.Infrastructure.Migrations.Application
{
    /// <inheritdoc />
    public partial class AddedResources2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "Resources",
                schema: "App",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ContainerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    URI = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Meta = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "Resources",
                schema: "App");
        }
    }
}
