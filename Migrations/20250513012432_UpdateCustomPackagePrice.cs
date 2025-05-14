using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomPackagePrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerFeedback_Customers_CustomerId",
                table: "CustomerFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerFeedback_Products_ProductId",
                table: "CustomerFeedback");

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
                name: "FK_OrderItem_Orders_OrderId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_ProductVariants_VariantId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Customers_CustomerId",
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
                name: "PK_OrderItem",
                table: "OrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomPackageUsageBasedPricing",
                table: "CustomPackageUsageBasedPricing");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerFeedback",
                table: "CustomerFeedback");

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
                name: "OrderItem",
                newName: "OrderItems");

            migrationBuilder.RenameTable(
                name: "CustomPackageUsageBasedPricing",
                newName: "CustomPackageSelectedUsageBasedPricing");

            migrationBuilder.RenameTable(
                name: "CustomerFeedback",
                newName: "CustomerFeedbacks");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_VariantId",
                table: "OrderItems",
                newName: "IX_OrderItems_VariantId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomPackageUsageBasedPricing_UsageBasedPricingId",
                table: "CustomPackageSelectedUsageBasedPricing",
                newName: "IX_CustomPackageSelectedUsageBasedPricing_UsageBasedPricingId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerFeedback_ProductId",
                table: "CustomerFeedbacks",
                newName: "IX_CustomerFeedbacks_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerFeedback_CustomerId",
                table: "CustomerFeedbacks",
                newName: "IX_CustomerFeedbacks_CustomerId");

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

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Scope",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerGroupMembers",
                table: "CustomerGroupMembers",
                columns: new[] { "GroupId", "CustomerId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Scope",
                table: "Scope",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "OrderItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomPackageSelectedUsageBasedPricing",
                table: "CustomPackageSelectedUsageBasedPricing",
                columns: new[] { "PricingPackageId", "UsageBasedPricingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerFeedbacks",
                table: "CustomerFeedbacks",
                column: "FeedbackId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerFeedbacks_Customers_CustomerId",
                table: "CustomerFeedbacks",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerFeedbacks_Products_ProductId",
                table: "CustomerFeedbacks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPackageSelectedUsageBasedPricing_PricingPackages_Pric~",
                table: "CustomPackageSelectedUsageBasedPricing",
                column: "PricingPackageId",
                principalTable: "PricingPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPackageSelectedUsageBasedPricing_UsageBasedPricing_Us~",
                table: "CustomPackageSelectedUsageBasedPricing",
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
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductVariants_VariantId",
                table: "OrderItems",
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
                onDelete: ReferentialAction.Restrict);

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
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Customers_CustomerId",
                table: "Sales",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerFeedbacks_Customers_CustomerId",
                table: "CustomerFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerFeedbacks_Products_ProductId",
                table: "CustomerFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomPackageSelectedUsageBasedPricing_PricingPackages_Pric~",
                table: "CustomPackageSelectedUsageBasedPricing");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomPackageSelectedUsageBasedPricing_UsageBasedPricing_Us~",
                table: "CustomPackageSelectedUsageBasedPricing");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Scope_StoreId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductVariants_VariantId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Customers_CustomerId",
                table: "Sales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerGroupMembers",
                table: "CustomerGroupMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Scope",
                table: "Scope");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomPackageSelectedUsageBasedPricing",
                table: "CustomPackageSelectedUsageBasedPricing");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerFeedbacks",
                table: "CustomerFeedbacks");

            migrationBuilder.RenameTable(
                name: "Scope",
                newName: "Scopes");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "OrderItem");

            migrationBuilder.RenameTable(
                name: "CustomPackageSelectedUsageBasedPricing",
                newName: "CustomPackageUsageBasedPricing");

            migrationBuilder.RenameTable(
                name: "CustomerFeedbacks",
                newName: "CustomerFeedback");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_VariantId",
                table: "OrderItem",
                newName: "IX_OrderItem_VariantId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItem",
                newName: "IX_OrderItem_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomPackageSelectedUsageBasedPricing_UsageBasedPricingId",
                table: "CustomPackageUsageBasedPricing",
                newName: "IX_CustomPackageUsageBasedPricing_UsageBasedPricingId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerFeedbacks_ProductId",
                table: "CustomerFeedback",
                newName: "IX_CustomerFeedback_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerFeedbacks_CustomerId",
                table: "CustomerFeedback",
                newName: "IX_CustomerFeedback_CustomerId");

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

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Scopes",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerGroupMembers",
                table: "CustomerGroupMembers",
                column: "MembershipId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Scopes",
                table: "Scopes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem",
                column: "OrderItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomPackageUsageBasedPricing",
                table: "CustomPackageUsageBasedPricing",
                columns: new[] { "PricingPackageId", "UsageBasedPricingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerFeedback",
                table: "CustomerFeedback",
                column: "FeedbackId");

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
                name: "FK_CustomerFeedback_Customers_CustomerId",
                table: "CustomerFeedback",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerFeedback_Products_ProductId",
                table: "CustomerFeedback",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
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
                name: "FK_Sales_Customers_CustomerId",
                table: "Sales",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId");
        }
    }
}
