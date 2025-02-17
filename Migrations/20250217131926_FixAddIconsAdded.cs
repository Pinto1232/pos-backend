using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixAddIconsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddOns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddOns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoreFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreFeatures", x => x.Id);
                });

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
                    TestPeriodDays = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsageBasedPricing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FeatureId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    MinValue = table.Column<int>(type: "integer", nullable: false),
                    MaxValue = table.Column<int>(type: "integer", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageBasedPricing", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AddOns",
                columns: new[] { "Id", "Description", "Name", "Price" },
                values: new object[,]
                {
                    { 201, "24/7 priority support via chat and email.", "Premium Support", 5.00m },
                    { 202, "Add your own logo and color scheme to the POS.", "Custom Branding", 7.00m }
                });

            migrationBuilder.InsertData(
                table: "CoreFeatures",
                columns: new[] { "Id", "BasePrice", "Description", "IsRequired", "Name" },
                values: new object[,]
                {
                    { 101, 10.00m, "Track and manage your inventory in real-time.", true, "Inventory Management" },
                    { 102, 8.00m, "Generate detailed reports on sales and revenue.", false, "Sales Reporting" },
                    { 103, 12.00m, "Manage multiple store locations from one dashboard.", false, "Multi-Location Support" }
                });

            migrationBuilder.InsertData(
                table: "PricingPackages",
                columns: new[] { "Id", "Description", "ExtraDescription", "Icon", "Price", "TestPeriodDays", "Title", "Type" },
                values: new object[,]
                {
                    { 1, "Select the essential modules and features for your business.;Ideal for small businesses or those new to POS systems.", "This package is perfect for startups and small businesses.", "StartIcon", 29.99m, 14, "Starter", "starter" },
                    { 2, "Expand your business capabilities with advanced modules and features.;Designed for growing businesses looking to enhance their POS system.", "Ideal for businesses looking to scale and grow.", "TrendingUpIcon", 59.99m, 14, "Growth", "growth" },
                    { 3, "Tailor-made solutions for your unique business needs.;Perfect for businesses requiring customized POS features.", "Get a POS system that fits your specific requirements.", "BuildIcon", 99.99m, 30, "Custom", "custom" },
                    { 4, "Comprehensive POS solutions for large enterprises.;Includes all advanced features and premium support.", "Ideal for large businesses with extensive POS needs.", "BusinessIcon", 199.99m, 30, "Enterprise", "enterprise" },
                    { 5, "All-inclusive POS package with premium features.;Best for businesses looking for top-tier POS solutions.", "Experience the best POS system with all features included.", "StarIcon", 299.99m, 30, "Premium", "premium" }
                });

            migrationBuilder.InsertData(
                table: "UsageBasedPricing",
                columns: new[] { "Id", "FeatureId", "MaxValue", "MinValue", "Name", "PricePerUnit", "Unit" },
                values: new object[,]
                {
                    { 1, 101, 100000, 1000, "API Calls", 0.01m, "requests" },
                    { 2, 102, 50, 1, "User Licenses", 5.00m, "users" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddOns");

            migrationBuilder.DropTable(
                name: "CoreFeatures");

            migrationBuilder.DropTable(
                name: "PricingPackages");

            migrationBuilder.DropTable(
                name: "UsageBasedPricing");
        }
    }
}
