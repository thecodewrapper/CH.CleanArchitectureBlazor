using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CH.CleanArchitecture.Infrastructure.Migrations.Application
{
    /// <inheritdoc />
    public partial class AddedResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Addresses_BillingAddressId",
                schema: "Domain",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Addresses_ShippingAddressId",
                schema: "Domain",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "Area",
                schema: "Domain",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlatNumber",
                schema: "Domain",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Addresses_BillingAddressId",
                schema: "Domain",
                table: "Orders",
                column: "BillingAddressId",
                principalSchema: "Domain",
                principalTable: "Addresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Addresses_ShippingAddressId",
                schema: "Domain",
                table: "Orders",
                column: "ShippingAddressId",
                principalSchema: "Domain",
                principalTable: "Addresses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Addresses_BillingAddressId",
                schema: "Domain",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Addresses_ShippingAddressId",
                schema: "Domain",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Area",
                schema: "Domain",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "FlatNumber",
                schema: "Domain",
                table: "Addresses");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Addresses_BillingAddressId",
                schema: "Domain",
                table: "Orders",
                column: "BillingAddressId",
                principalSchema: "Domain",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Addresses_ShippingAddressId",
                schema: "Domain",
                table: "Orders",
                column: "ShippingAddressId",
                principalSchema: "Domain",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
