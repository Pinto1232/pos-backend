using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                table: "Currencies",
                columns: new[] { "Code", "ExchangeRate" },
                values: new object[,]
                {
                    { "EUR", 0.9m },
                    { "GBP", 0.8m },
                    { "USD", 1.0m }
                });

            migrationBuilder.InsertData(
                table: "PricingPackages",
                columns: new[] { "Id", "Currency", "Description", "ExtraDescription", "Icon", "MultiCurrencyPrices", "Price", "TestPeriodDays", "Title", "Type" },
                values: new object[,]
                {
                    { 1, "", "Select the essential modules and features for your business.;Ideal for small businesses or those new to POS systems.", "This package is perfect for startups and small businesses.", "MUI:StartIcon", "{}", 29.99m, 14, "Starter", "starter" },
                    { 2, "", "Expand your business with advanced features.;Perfect for growing businesses with multiple products.", "Scale your business with our growth package.", "MUI:TrendingUpIcon", "{}", 59.99m, 14, "Growth", "growth" },
                    { 3, "", "Build your own package with the features you need.;Pay only for what your business requires.", "Customize your POS experience.", "MUI:BuildIcon", "{}", 39.99m, 14, "Custom", "custom" },
                    { 4, "", "Comprehensive POS solutions for large enterprises.;Includes all advanced features and premium support.", "Ideal for large businesses with extensive POS needs.", "MUI:BusinessIcon", "{}", 199.99m, 30, "Enterprise", "enterprise" },
                    { 5, "", "All-inclusive POS package with premium features.;Best for businesses looking for top-tier POS solutions.", "Experience the best POS system with all features included.", "MUI:StarIcon", "{}", 299.99m, 30, "Premium", "premium" }
                });

            migrationBuilder.InsertData(
                table: "UsageBasedPricing",
                columns: new[] { "Id", "FeatureId", "MaxValue", "MinValue", "Name", "PricePerUnit", "Unit" },
                values: new object[,]
                {
                    { 1, 101, 10000, 100, "Number of Products", 0.05m, "products" },
                    { 2, 103, 50, 1, "Number of Locations", 5.00m, "locations" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AddOns",
                keyColumn: "Id",
                keyValue: 201);

            migrationBuilder.DeleteData(
                table: "AddOns",
                keyColumn: "Id",
                keyValue: 202);

            migrationBuilder.DeleteData(
                table: "CoreFeatures",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "CoreFeatures",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "CoreFeatures",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "EUR");

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "GBP");

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "USD");

            migrationBuilder.DeleteData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PricingPackages",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "UsageBasedPricing",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UsageBasedPricing",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
