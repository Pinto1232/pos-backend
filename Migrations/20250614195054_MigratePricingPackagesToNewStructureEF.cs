﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class MigratePricingPackagesToNewStructureEF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migration script to migrate existing pricing data from PricingPackages.Price to PackagePrices table
            // This fixes the issue where packages show $0.00 because the new pricing structure isn't populated

            // Insert base USD prices from the legacy Price field
            migrationBuilder.Sql(@"
                INSERT INTO ""PackagePrices"" (""PackageId"", ""Currency"", ""Price"", ""CreatedAt"")
                SELECT 
                    ""Id"" as ""PackageId"",
                    'USD' as ""Currency"",
                    ""Price"",
                    NOW() as ""CreatedAt""
                FROM ""PricingPackages""
                WHERE ""Price"" > 0
                    AND NOT EXISTS (
                        SELECT 1 FROM ""PackagePrices"" pp 
                        WHERE pp.""PackageId"" = ""PricingPackages"".""Id"" 
                        AND pp.""Currency"" = 'USD'
                    );
            ");

            // Parse and insert multi-currency prices from MultiCurrencyPrices JSON field
            // This handles the JSON data like: {""EUR"": 37.19, ""GBP"": 31.99, ""CAD"": 53.99, ""AUD"": 59.99, ""JPY"": 5999}

            // EUR prices
            migrationBuilder.Sql(@"
                INSERT INTO ""PackagePrices"" (""PackageId"", ""Currency"", ""Price"", ""CreatedAt"")
                SELECT 
                    ""Id"" as ""PackageId"",
                    'EUR' as ""Currency"",
                    CAST(json_extract_path_text(""MultiCurrencyPrices""::json, 'EUR') AS DECIMAL(18,2)) as ""Price"",
                    NOW() as ""CreatedAt""
                FROM ""PricingPackages""
                WHERE ""MultiCurrencyPrices"" IS NOT NULL 
                    AND ""MultiCurrencyPrices"" != ''
                    AND ""MultiCurrencyPrices"" != '{}'
                    AND json_extract_path_text(""MultiCurrencyPrices""::json, 'EUR') IS NOT NULL
                    AND json_extract_path_text(""MultiCurrencyPrices""::json, 'EUR') != ''
                    AND NOT EXISTS (
                        SELECT 1 FROM ""PackagePrices"" pp 
                        WHERE pp.""PackageId"" = ""PricingPackages"".""Id"" 
                        AND pp.""Currency"" = 'EUR'
                    );
            ");

            // GBP prices
            migrationBuilder.Sql(@"
                INSERT INTO ""PackagePrices"" (""PackageId"", ""Currency"", ""Price"", ""CreatedAt"")
                SELECT 
                    ""Id"" as ""PackageId"",
                    'GBP' as ""Currency"",
                    CAST(json_extract_path_text(""MultiCurrencyPrices""::json, 'GBP') AS DECIMAL(18,2)) as ""Price"",
                    NOW() as ""CreatedAt""
                FROM ""PricingPackages""
                WHERE ""MultiCurrencyPrices"" IS NOT NULL 
                    AND ""MultiCurrencyPrices"" != ''
                    AND ""MultiCurrencyPrices"" != '{}'
                    AND json_extract_path_text(""MultiCurrencyPrices""::json, 'GBP') IS NOT NULL
                    AND json_extract_path_text(""MultiCurrencyPrices""::json, 'GBP') != ''
                    AND NOT EXISTS (
                        SELECT 1 FROM ""PackagePrices"" pp 
                        WHERE pp.""PackageId"" = ""PricingPackages"".""Id"" 
                        AND pp.""Currency"" = 'GBP'
                    );
            ");

            // CAD prices
            migrationBuilder.Sql(@"
                INSERT INTO ""PackagePrices"" (""PackageId"", ""Currency"", ""Price"", ""CreatedAt"")
                SELECT 
                    ""Id"" as ""PackageId"",
                    'CAD' as ""Currency"",
                    CAST(json_extract_path_text(""MultiCurrencyPrices""::json, 'CAD') AS DECIMAL(18,2)) as ""Price"",
                    NOW() as ""CreatedAt""
                FROM ""PricingPackages""
                WHERE ""MultiCurrencyPrices"" IS NOT NULL 
                    AND ""MultiCurrencyPrices"" != ''
                    AND ""MultiCurrencyPrices"" != '{}'
                    AND json_extract_path_text(""MultiCurrencyPrices""::json, 'CAD') IS NOT NULL
                    AND json_extract_path_text(""MultiCurrencyPrices""::json, 'CAD') != ''
                    AND NOT EXISTS (
                        SELECT 1 FROM ""PackagePrices"" pp 
                        WHERE pp.""PackageId"" = ""PricingPackages"".""Id"" 
                        AND pp.""Currency"" = 'CAD'
                    );
            ");

            // AUD prices
            migrationBuilder.Sql(@"
                INSERT INTO ""PackagePrices"" (""PackageId"", ""Currency"", ""Price"", ""CreatedAt"")
                SELECT 
                    ""Id"" as ""PackageId"",
                    'AUD' as ""Currency"",
                    CAST(json_extract_path_text(""MultiCurrencyPrices""::json, 'AUD') AS DECIMAL(18,2)) as ""Price"",
                    NOW() as ""CreatedAt""
                FROM ""PricingPackages""
                WHERE ""MultiCurrencyPrices"" IS NOT NULL 
                    AND ""MultiCurrencyPrices"" != ''
                    AND ""MultiCurrencyPrices"" != '{}'
                    AND json_extract_path_text(""MultiCurrencyPrices""::json, 'AUD') IS NOT NULL
                    AND json_extract_path_text(""MultiCurrencyPrices""::json, 'AUD') != ''
                    AND NOT EXISTS (
                        SELECT 1 FROM ""PackagePrices"" pp 
                        WHERE pp.""PackageId"" = ""PricingPackages"".""Id"" 
                        AND pp.""Currency"" = 'AUD'
                    );
            ");

            // JPY prices
            migrationBuilder.Sql(@"
                INSERT INTO ""PackagePrices"" (""PackageId"", ""Currency"", ""Price"", ""CreatedAt"")
                SELECT 
                    ""Id"" as ""PackageId"",
                    'JPY' as ""Currency"",
                    CAST(json_extract_path_text(""MultiCurrencyPrices""::json, 'JPY') AS DECIMAL(18,2)) as ""Price"",
                    NOW() as ""CreatedAt""
                FROM ""PricingPackages""
                WHERE ""MultiCurrencyPrices"" IS NOT NULL 
                    AND ""MultiCurrencyPrices"" != ''
                    AND ""MultiCurrencyPrices"" != '{}'
                    AND json_extract_path_text(""MultiCurrencyPrices""::json, 'JPY') IS NOT NULL
                    AND json_extract_path_text(""MultiCurrencyPrices""::json, 'JPY') != ''
                    AND NOT EXISTS (
                        SELECT 1 FROM ""PackagePrices"" pp 
                        WHERE pp.""PackageId"" = ""PricingPackages"".""Id"" 
                        AND pp.""Currency"" = 'JPY'
                    );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This is a data migration - we don't reverse the data changes
            // as it would cause data loss. The PackagePrices table should 
            // remain populated to ensure pricing continues to work.
            
            // If rollback is absolutely necessary, run this SQL manually:
            // DELETE FROM "PackagePrices" WHERE "CreatedAt" >= '{migration_date}';
        }
    }
}
