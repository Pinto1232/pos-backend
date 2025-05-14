using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using PosBackend.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PosBackend
{
    class UpdateCustomPackage
    {
        // Renamed from Main to avoid entry point conflict
        public static async Task UpdatePackage()
        {
            Console.WriteLine("Starting Custom package price update...");

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Connection string not found in appsettings.json");
                return;
            }

            Console.WriteLine("Creating DbContext...");

            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<PosDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            // Suppress the pending model changes warning
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

            // Create DbContext
            using (var context = new PosDbContext(optionsBuilder.Options))
            {
                Console.WriteLine("Updating Custom package price...");

                try
                {
                    // Check if the PricingPackages table exists
                    bool tableExists = true;
                    try
                    {
                        var count = await context.PricingPackages.CountAsync();
                        Console.WriteLine($"Found {count} pricing packages in the database.");
                    }
                    catch (Exception ex)
                    {
                        tableExists = false;
                        Console.WriteLine($"Error accessing PricingPackages table: {ex.Message}");
                    }

                    if (!tableExists)
                    {
                        Console.WriteLine("The PricingPackages table doesn't exist. Creating it using raw SQL...");
                        try
                        {
                            // Execute raw SQL to create the table and insert the Custom package
                            await context.Database.ExecuteSqlRawAsync(@"
                                CREATE TABLE IF NOT EXISTS ""PricingPackages"" (
                                    ""Id"" SERIAL PRIMARY KEY,
                                    ""Title"" TEXT NOT NULL,
                                    ""Description"" TEXT NOT NULL,
                                    ""Icon"" TEXT NOT NULL,
                                    ""ExtraDescription"" TEXT NOT NULL,
                                    ""Price"" NUMERIC NOT NULL,
                                    ""TestPeriodDays"" INTEGER NOT NULL,
                                    ""Type"" TEXT NOT NULL,
                                    ""Currency"" TEXT NOT NULL,
                                    ""MultiCurrencyPrices"" TEXT NOT NULL
                                );

                                INSERT INTO ""PricingPackages"" (""Title"", ""Description"", ""Icon"", ""ExtraDescription"", ""Price"", ""TestPeriodDays"", ""Type"", ""Currency"", ""MultiCurrencyPrices"")
                                VALUES ('Custom', 'Build your own package;Select only what you need;Flexible pricing;Scalable solution;Pay for what you use', 'MUI:SettingsIcon', 'Create a custom solution that fits your exact needs', 49.99, 14, 'custom', 'USD', '{""ZAR"": 899.99, ""EUR"": 45.99, ""GBP"": 39.99}')
                                ON CONFLICT (""Type"") DO UPDATE
                                SET ""Price"" = 49.99, ""MultiCurrencyPrices"" = '{""ZAR"": 899.99, ""EUR"": 45.99, ""GBP"": 39.99}';
                            ");
                            Console.WriteLine("Successfully created PricingPackages table and inserted Custom package.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error creating table: {ex.Message}");
                        }
                    }
                    else
                    {
                        // Find the Custom package
                        var customPackage = await context.PricingPackages
                            .FirstOrDefaultAsync(p => p.Type == "custom");

                        if (customPackage != null)
                        {
                            Console.WriteLine($"Found Custom package with current price: {customPackage.Price}");

                            // Update the price
                            customPackage.Price = 49.99m;
                            customPackage.MultiCurrencyPrices = "{\"ZAR\": 899.99, \"EUR\": 45.99, \"GBP\": 39.99}";

                            // Save changes
                            await context.SaveChangesAsync();
                            Console.WriteLine("Custom package price updated successfully to 49.99.");
                        }
                        else
                        {
                            Console.WriteLine("Custom package not found in the database. Trying to insert it...");

                            // Create a new Custom package
                            var newCustomPackage = new PricingPackage
                            {
                                Title = "Custom",
                                Description = "Build your own package;Select only what you need;Flexible pricing;Scalable solution;Pay for what you use",
                                Icon = "MUI:SettingsIcon",
                                ExtraDescription = "Create a custom solution that fits your exact needs",
                                Price = 49.99m,
                                TestPeriodDays = 14,
                                Type = "custom",
                                Currency = "USD",
                                MultiCurrencyPrices = "{\"ZAR\": 899.99, \"EUR\": 45.99, \"GBP\": 39.99}"
                            };

                            // Add and save the new package
                            context.PricingPackages.Add(newCustomPackage);
                            await context.SaveChangesAsync();
                            Console.WriteLine("Custom package created successfully with price 49.99.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating Custom package: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }

            Console.WriteLine("Update completed.");
        }
    }
}
