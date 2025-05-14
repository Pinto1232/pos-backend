using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldsToAddOns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "AddOns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "AddOns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AddOns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MultiCurrencyPrices",
                table: "AddOns",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "MultiCurrencyPrices",
                table: "AddOns");
        }
    }
}
