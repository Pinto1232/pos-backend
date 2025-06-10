using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.Json;

namespace PosBackend.Migrations
{
    public partial class SeedAddOnsData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Advanced Analytics
            migrationBuilder.InsertData(
                table: "AddOns",
                columns: new[] { "Id", "Name", "Description", "Price", "Currency", "MultiCurrencyPrices", "Category", "IsActive", "Features", "Dependencies", "Icon" },
                values: new object[] { 
                    1, 
                    "Advanced Analytics", 
                    "Gain deeper insights into your business with advanced analytics tools and dashboards.", 
                    29.99m, 
                    "USD", 
                    "{\"EUR\": 27.99, \"GBP\": 23.99, \"CAD\": 39.99, \"AUD\": 44.99}", 
                    "Business Intelligence", 
                    true, 
                    JsonSerializer.Serialize(new[] { "Real-time dashboard", "Custom report builder", "Data visualization tools", "Performance metrics", "Export to Excel/PDF" }), 
                    "[]", 
                    "analytics_icon" 
                });

            // API Access
            migrationBuilder.InsertData(
                table: "AddOns",
                columns: new[] { "Id", "Name", "Description", "Price", "Currency", "MultiCurrencyPrices", "Category", "IsActive", "Features", "Dependencies", "Icon" },
                values: new object[] { 
                    2, 
                    "API Access", 
                    "Connect your POS system with other applications through our comprehensive API.", 
                    39.99m, 
                    "USD", 
                    "{\"EUR\": 36.99, \"GBP\": 31.99, \"CAD\": 52.99, \"AUD\": 59.99}", 
                    "Integration", 
                    true, 
                    JsonSerializer.Serialize(new[] { "RESTful API endpoints", "OAuth authentication", "Comprehensive documentation", "Rate limits up to 10,000 requests/day", "Webhook support" }), 
                    "[]", 
                    "api_icon" 
                });

            // Custom Branding
            migrationBuilder.InsertData(
                table: "AddOns",
                columns: new[] { "Id", "Name", "Description", "Price", "Currency", "MultiCurrencyPrices", "Category", "IsActive", "Features", "Dependencies", "Icon" },
                values: new object[] { 
                    3, 
                    "Custom Branding", 
                    "Personalize your POS system with your brand colors, logo, and custom receipt designs.", 
                    19.99m, 
                    "USD", 
                    "{\"EUR\": 17.99, \"GBP\": 15.99, \"CAD\": 26.99, \"AUD\": 29.99}", 
                    "Customization", 
                    true, 
                    JsonSerializer.Serialize(new[] { "Custom logo", "Color scheme customization", "Receipt customization", "Email template branding", "Custom domain" }), 
                    "[]", 
                    "branding_icon" 
                });

            // 24/7 Support
            migrationBuilder.InsertData(
                table: "AddOns",
                columns: new[] { "Id", "Name", "Description", "Price", "Currency", "MultiCurrencyPrices", "Category", "IsActive", "Features", "Dependencies", "Icon" },
                values: new object[] { 
                    4, 
                    "24/7 Support", 
                    "Get priority support from our team of experts available around the clock.", 
                    49.99m, 
                    "USD", 
                    "{\"EUR\": 45.99, \"GBP\": 39.99, \"CAD\": 64.99, \"AUD\": 74.99}", 
                    "Support", 
                    true, 
                    JsonSerializer.Serialize(new[] { "Phone support", "Live chat", "Priority email", "Dedicated account manager", "Monthly check-ins" }), 
                    "[]", 
                    "support_icon" 
                });

            // Data Migration
            migrationBuilder.InsertData(
                table: "AddOns",
                columns: new[] { "Id", "Name", "Description", "Price", "Currency", "MultiCurrencyPrices", "Category", "IsActive", "Features", "Dependencies", "Icon" },
                values: new object[] { 
                    5, 
                    "Data Migration", 
                    "Seamlessly transfer your data from other systems to our POS platform.", 
                    99.99m, 
                    "USD", 
                    "{\"EUR\": 89.99, \"GBP\": 79.99, \"CAD\": 129.99, \"AUD\": 149.99}", 
                    "Services", 
                    true, 
                    JsonSerializer.Serialize(new[] { "Data mapping", "Automated transfer", "Data validation", "Historical data import", "Custom field mapping" }), 
                    "[]", 
                    "migration_icon" 
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AddOns",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AddOns",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AddOns",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AddOns",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AddOns",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}