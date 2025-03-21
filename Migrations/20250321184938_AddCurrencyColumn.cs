using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MultiCurrencyPrices",
                table: "PricingPackages",
                type: "text",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "PricingPackages",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldNullable: true,
                oldDefaultValue: "USD");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "Currency",
                value: "");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "Currency",
                value: "");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "Currency",
                value: "");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "Currency",
                value: "");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 5,
                column: "Currency",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MultiCurrencyPrices",
                table: "PricingPackages",
                type: "text",
                nullable: true,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "PricingPackages",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "USD");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "Currency",
                value: "USD");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "Currency",
                value: "USD");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "Currency",
                value: "USD");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "Currency",
                value: "USD");

            migrationBuilder.UpdateData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 5,
                column: "Currency",
                value: "USD");
        }
    }
}
