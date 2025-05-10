using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingDbContextEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomPackageSelectedFeatures_CoreFeatures_FeatureId",
                table: "CustomPackageSelectedFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomPackageUsageBasedPricing_PricingPackages_PricingPacka~",
                table: "CustomPackageUsageBasedPricing");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomPackageUsageBasedPricing_UsageBasedPricing_UsageBased~",
                table: "CustomPackageUsageBasedPricing");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Scopes_StoreId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Sales_SaleId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Orders_OrderId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_ProductVariants_VariantId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductExpiries_ProductVariants_VariantId",
                table: "ProductExpiries");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductExpiries_Products_ProductId",
                table: "ProductExpiries");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_ProductVariants_VariantId",
                table: "SaleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Customers_CustomerId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Terminals_TerminalId",
                table: "Sales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerGroupMembers",
                table: "CustomerGroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerGroupMembers_GroupId",
                table: "CustomerGroupMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Scopes",
                table: "Scopes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductExpiries",
                table: "ProductExpiries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Invoices",
                table: "Invoices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomPackageUsageBasedPricing",
                table: "CustomPackageUsageBasedPricing");

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

            migrationBuilder.RenameTable(
                name: "Scopes",
                newName: "Scope");

            migrationBuilder.RenameTable(
                name: "ProductExpiries",
                newName: "ProductExpiry");

            migrationBuilder.RenameTable(
                name: "OrderItem",
                newName: "OrderItems");

            migrationBuilder.RenameTable(
                name: "Invoices",
                newName: "Invoice");

            migrationBuilder.RenameTable(
                name: "CustomPackageUsageBasedPricing",
                newName: "CustomPackageUsageBasedPricings");

            migrationBuilder.RenameIndex(
                name: "IX_ProductExpiries_VariantId",
                table: "ProductExpiry",
                newName: "IX_ProductExpiry_VariantId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductExpiries_ProductId",
                table: "ProductExpiry",
                newName: "IX_ProductExpiry_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_VariantId",
                table: "OrderItems",
                newName: "IX_OrderItems_VariantId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_SaleId",
                table: "Invoice",
                newName: "IX_Invoice_SaleId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomPackageUsageBasedPricing_UsageBasedPricingId",
                table: "CustomPackageUsageBasedPricings",
                newName: "IX_CustomPackageUsageBasedPricings_UsageBasedPricingId");

            migrationBuilder.AlterColumn<string>(
                name: "TaxSettingsJson",
                table: "UserCustomizations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RegionalSettingsJson",
                table: "UserCustomizations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MultiCurrencyPrices",
                table: "PricingPackages",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "PricingPackages",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "USD");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "Currencies",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AddColumn<int>(
                name: "ProductVariantVariantId",
                table: "OrderItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CustomPackageUsageBasedPricings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerGroupMembers",
                table: "CustomerGroupMembers",
                columns: new[] { "GroupId", "CustomerId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Scope",
                table: "Scope",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductExpiry",
                table: "ProductExpiry",
                column: "ExpiryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "OrderItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Invoice",
                table: "Invoice",
                column: "InvoiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomPackageUsageBasedPricings",
                table: "CustomPackageUsageBasedPricings",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Features",
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
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductVariantVariantId",
                table: "OrderItems",
                column: "ProductVariantVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomPackageUsageBasedPricings_PricingPackageId",
                table: "CustomPackageUsageBasedPricings",
                column: "PricingPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPackageSelectedFeatures_Features_FeatureId",
                table: "CustomPackageSelectedFeatures",
                column: "FeatureId",
                principalTable: "Features",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPackageUsageBasedPricings_PricingPackages_PricingPack~",
                table: "CustomPackageUsageBasedPricings",
                column: "PricingPackageId",
                principalTable: "PricingPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPackageUsageBasedPricings_UsageBasedPricing_UsageBase~",
                table: "CustomPackageUsageBasedPricings",
                column: "UsageBasedPricingId",
                principalTable: "UsageBasedPricing",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Scope_StoreId",
                table: "Inventories",
                column: "StoreId",
                principalTable: "Scope",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Sales_SaleId",
                table: "Invoice",
                column: "SaleId",
                principalTable: "Sales",
                principalColumn: "SaleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductVariants_ProductVariantVariantId",
                table: "OrderItems",
                column: "ProductVariantVariantId",
                principalTable: "ProductVariants",
                principalColumn: "VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductVariants_VariantId",
                table: "OrderItems",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "VariantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductExpiry_ProductVariants_VariantId",
                table: "ProductExpiry",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "VariantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductExpiry_Products_ProductId",
                table: "ProductExpiry",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_ProductVariants_VariantId",
                table: "SaleItems",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "VariantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Customers_CustomerId",
                table: "Sales",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Terminals_TerminalId",
                table: "Sales",
                column: "TerminalId",
                principalTable: "Terminals",
                principalColumn: "TerminalId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomPackageSelectedFeatures_Features_FeatureId",
                table: "CustomPackageSelectedFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomPackageUsageBasedPricings_PricingPackages_PricingPack~",
                table: "CustomPackageUsageBasedPricings");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomPackageUsageBasedPricings_UsageBasedPricing_UsageBase~",
                table: "CustomPackageUsageBasedPricings");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Scope_StoreId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Sales_SaleId",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductVariants_ProductVariantVariantId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductVariants_VariantId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductExpiry_ProductVariants_VariantId",
                table: "ProductExpiry");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductExpiry_Products_ProductId",
                table: "ProductExpiry");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_ProductVariants_VariantId",
                table: "SaleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Customers_CustomerId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Terminals_TerminalId",
                table: "Sales");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerGroupMembers",
                table: "CustomerGroupMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Scope",
                table: "Scope");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductExpiry",
                table: "ProductExpiry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductVariantVariantId",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Invoice",
                table: "Invoice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomPackageUsageBasedPricings",
                table: "CustomPackageUsageBasedPricings");

            migrationBuilder.DropIndex(
                name: "IX_CustomPackageUsageBasedPricings_PricingPackageId",
                table: "CustomPackageUsageBasedPricings");

            migrationBuilder.DropColumn(
                name: "ProductVariantVariantId",
                table: "OrderItems");

            migrationBuilder.RenameTable(
                name: "Scope",
                newName: "Scopes");

            migrationBuilder.RenameTable(
                name: "ProductExpiry",
                newName: "ProductExpiries");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "OrderItem");

            migrationBuilder.RenameTable(
                name: "Invoice",
                newName: "Invoices");

            migrationBuilder.RenameTable(
                name: "CustomPackageUsageBasedPricings",
                newName: "CustomPackageUsageBasedPricing");

            migrationBuilder.RenameIndex(
                name: "IX_ProductExpiry_VariantId",
                table: "ProductExpiries",
                newName: "IX_ProductExpiries_VariantId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductExpiry_ProductId",
                table: "ProductExpiries",
                newName: "IX_ProductExpiries_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_VariantId",
                table: "OrderItem",
                newName: "IX_OrderItem_VariantId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItem",
                newName: "IX_OrderItem_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoice_SaleId",
                table: "Invoices",
                newName: "IX_Invoices_SaleId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomPackageUsageBasedPricings_UsageBasedPricingId",
                table: "CustomPackageUsageBasedPricing",
                newName: "IX_CustomPackageUsageBasedPricing_UsageBasedPricingId");

            migrationBuilder.AlterColumn<string>(
                name: "TaxSettingsJson",
                table: "UserCustomizations",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RegionalSettingsJson",
                table: "UserCustomizations",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MultiCurrencyPrices",
                table: "PricingPackages",
                type: "text",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "PricingPackages",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "Currencies",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CustomPackageUsageBasedPricing",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerGroupMembers",
                table: "CustomerGroupMembers",
                column: "MembershipId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Scopes",
                table: "Scopes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductExpiries",
                table: "ProductExpiries",
                column: "ExpiryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem",
                column: "OrderItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Invoices",
                table: "Invoices",
                column: "InvoiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomPackageUsageBasedPricing",
                table: "CustomPackageUsageBasedPricing",
                columns: new[] { "PricingPackageId", "UsageBasedPricingId" });

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
                    { 2, "", "Expand your business capabilities with advanced modules and features.;Designed for growing businesses looking to enhance their POS system.", "Ideal for businesses looking to scale and grow.", "MUI:TrendingUpIcon", "{}", 59.99m, 14, "Growth", "growth" },
                    { 3, "", "Tailor-made solutions for your unique business needs.;Perfect for businesses requiring customized POS features.", "Get a POS system that fits your specific requirements.", "MUI:BuildIcon", "{}", 99.99m, 30, "Custom", "custom" },
                    { 4, "", "Comprehensive POS solutions for large enterprises.;Includes all advanced features and premium support.", "Ideal for large businesses with extensive POS needs.", "MUI:BusinessIcon", "{}", 199.99m, 30, "Enterprise", "enterprise" },
                    { 5, "", "All-inclusive POS package with premium features.;Best for businesses looking for top-tier POS solutions.", "Experience the best POS system with all features included.", "MUI:StarIcon", "{}", 299.99m, 30, "Premium", "premium" }
                });

            migrationBuilder.InsertData(
                table: "UsageBasedPricing",
                columns: new[] { "Id", "FeatureId", "MaxValue", "MinValue", "Name", "PricePerUnit", "Unit" },
                values: new object[,]
                {
                    { 1, 101, 100000, 1000, "API Calls", 0.01m, "requests" },
                    { 2, 102, 50, 1, "User Licenses", 5.00m, "users" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerGroupMembers_GroupId",
                table: "CustomerGroupMembers",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPackageSelectedFeatures_CoreFeatures_FeatureId",
                table: "CustomPackageSelectedFeatures",
                column: "FeatureId",
                principalTable: "CoreFeatures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPackageUsageBasedPricing_PricingPackages_PricingPacka~",
                table: "CustomPackageUsageBasedPricing",
                column: "PricingPackageId",
                principalTable: "PricingPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPackageUsageBasedPricing_UsageBasedPricing_UsageBased~",
                table: "CustomPackageUsageBasedPricing",
                column: "UsageBasedPricingId",
                principalTable: "UsageBasedPricing",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Scopes_StoreId",
                table: "Inventories",
                column: "StoreId",
                principalTable: "Scopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Sales_SaleId",
                table: "Invoices",
                column: "SaleId",
                principalTable: "Sales",
                principalColumn: "SaleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Orders_OrderId",
                table: "OrderItem",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_ProductVariants_VariantId",
                table: "OrderItem",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "VariantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductExpiries_ProductVariants_VariantId",
                table: "ProductExpiries",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "VariantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductExpiries_Products_ProductId",
                table: "ProductExpiries",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_ProductVariants_VariantId",
                table: "SaleItems",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "VariantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Customers_CustomerId",
                table: "Sales",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Terminals_TerminalId",
                table: "Sales",
                column: "TerminalId",
                principalTable: "Terminals",
                principalColumn: "TerminalId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
