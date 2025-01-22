using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PricingPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    ExtraDescription = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    TestPeriodDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingPackages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PricingPackages",
                columns: new[] { "Id", "Description", "ExtraDescription", "Icon", "Price", "TestPeriodDays", "Title" },
                values: new object[,]
                {
                    { 1, "Select the essential modules and features for your business.;Ideal for small businesses or those new to POS systems.", "This package is perfect for startups and small businesses.", "starter-icon.png", 29.99m, 14, "Starter" },
                    { 2, "Expand your business capabilities with advanced modules and features.;Designed for growing businesses looking to enhance their POS system.", "Ideal for businesses looking to scale and grow.", "growth-icon.png", 59.99m, 14, "Growth" },
                    { 3, "Tailor-made solutions for your unique business needs.;Perfect for businesses requiring customized POS features.", "Get a POS system that fits your specific requirements.", "custom-icon.png", 99.99m, 30, "Custom" },
                    { 4, "Comprehensive POS solutions for large enterprises.;Includes all advanced features and premium support.", "Ideal for large businesses with extensive POS needs.", "enterprise-icon.png", 199.99m, 30, "Enterprise" },
                    { 5, "All-inclusive POS package with premium features.;Best for businesses looking for top-tier POS solutions.", "Experience the best POS system with all features included.", "premium-icon.png", 299.99m, 30, "Premium" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingPackages");
        }
    }
}
