using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePricingPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PricingPackages",
                type: "text",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Select the essential modules and features for your business.;Ideal for small businesses or those new to POS systems.");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Expand your business capabilities with advanced modules and features.;Designed for growing businesses looking to enhance their POS system.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<string>>(
                name: "Description",
                table: "PricingPackages",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: new List<string> { "Select the essential modules and features for your business.", "Ideal for small businesses or those new to POS systems." });

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: new List<string> { "Expand your business capabilities with advanced modules and features.", "Designed for growing businesses looking to enhance their POS system." });
        }
    }
}
