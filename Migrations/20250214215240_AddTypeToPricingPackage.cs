using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeToPricingPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "PricingPackages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "Type",
                value: "starter");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "Type",
                value: "growth");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "Type",
                value: "custom");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "Type",
                value: "enterprise");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 5,
                column: "Type",
                value: "premium");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "PricingPackages");
        }
    }
}
