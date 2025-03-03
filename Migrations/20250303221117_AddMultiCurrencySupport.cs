using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiCurrencySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PricingPackages",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true,
                defaultValue: "USD");

            migrationBuilder.AddColumn<string>(
                name: "MultiCurrencyPrices",
                table: "PricingPackages",
                type: "text",
                nullable: true,
                defaultValue: "{}");

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Code);
                });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Code", "ExchangeRate" },
                values: new object[,]
                {
                    { "EUR", 0.9m },
                    { "GBP", 0.8m },
                    { "USD", 1.0m }
                });

            // Removed the empty UpdateData calls that were generating invalid SQL.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PricingPackages");

            migrationBuilder.DropColumn(
                name: "MultiCurrencyPrices",
                table: "PricingPackages");
        }
    }
}
