using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePackagePricingAndCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing packages with multi-currency pricing
            migrationBuilder.Sql(@"
                -- Update existing packages to include multi-currency pricing
                UPDATE ""PricingPackages"" 
                SET 
                    ""Currency"" = 'USD',
                    ""MultiCurrencyPrices"" = CASE 
                        WHEN ""Price"" = 0 THEN '{""EUR"": 0, ""GBP"": 0, ""CAD"": 0, ""AUD"": 0, ""JPY"": 0}'
                        WHEN ""Price"" <= 10 THEN '{""EUR"": ' || (""Price"" * 0.93)::numeric(10,2) || ', ""GBP"": ' || (""Price"" * 0.80)::numeric(10,2) || ', ""CAD"": ' || (""Price"" * 1.35)::numeric(10,2) || ', ""AUD"": ' || (""Price"" * 1.50)::numeric(10,2) || ', ""JPY"": ' || (""Price"" * 150)::numeric(10,0) || '}'
                        WHEN ""Price"" <= 50 THEN '{""EUR"": ' || (""Price"" * 0.93)::numeric(10,2) || ', ""GBP"": ' || (""Price"" * 0.80)::numeric(10,2) || ', ""CAD"": ' || (""Price"" * 1.35)::numeric(10,2) || ', ""AUD"": ' || (""Price"" * 1.50)::numeric(10,2) || ', ""JPY"": ' || (""Price"" * 150)::numeric(10,0) || '}'
                        ELSE '{""EUR"": ' || (""Price"" * 0.93)::numeric(10,2) || ', ""GBP"": ' || (""Price"" * 0.80)::numeric(10,2) || ', ""CAD"": ' || (""Price"" * 1.35)::numeric(10,2) || ', ""AUD"": ' || (""Price"" * 1.50)::numeric(10,2) || ', ""JPY"": ' || (""Price"" * 150)::numeric(10,0) || '}'
                    END
                WHERE ""Currency"" = '' OR ""Currency"" IS NULL OR ""MultiCurrencyPrices"" = '{}' OR ""MultiCurrencyPrices"" = '';

                -- Update specific package pricing if needed (example)
                -- Uncomment and modify as needed for your specific packages
                /*
                UPDATE ""PricingPackages"" 
                SET 
                    ""Price"" = 29.99,
                    ""MultiCurrencyPrices"" = '{""EUR"": 27.99, ""GBP"": 23.99, ""CAD"": 39.99, ""AUD"": 44.99, ""JPY"": 4500}'
                WHERE ""Title"" = 'Premium Package';
                
                UPDATE ""PricingPackages"" 
                SET 
                    ""Price"" = 49.99,
                    ""MultiCurrencyPrices"" = '{""EUR"": 46.99, ""GBP"": 39.99, ""CAD"": 67.99, ""AUD"": 74.99, ""JPY"": 7500}'
                WHERE ""Title"" = 'Enterprise Package';
                */
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert multi-currency pricing changes
            migrationBuilder.Sql(@"
                UPDATE ""PricingPackages"" 
                SET 
                    ""Currency"" = '',
                    ""MultiCurrencyPrices"" = '{}'
                WHERE ""Currency"" = 'USD';
            ");
        }
    }
}
