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
                                    ""MultiCurrencyPrices"" TEXT NOT NULL,
                                    ""TierLevel"" INTEGER DEFAULT 1
                                );

                                INSERT INTO ""PricingPackages"" (""Title"", ""Description"", ""Icon"", ""ExtraDescription"", ""Price"", ""TestPeriodDays"", ""Type"", ""Currency"", ""MultiCurrencyPrices"", ""TierLevel"")
                                VALUES ('Custom Pro', 'Flexible solutions tailored to unique requirements;Customizable features;Industry specific;Flexible scaling;Personalized onboarding;Custom workflows;Advanced integrations;Priority support', 'MUI:CustomIcon', 'Tailored solutions for unique business requirements', 149.99, 14, 'custom-pro', 'USD', '{""ZAR"": 2749.99, ""EUR"": 139.99, ""GBP"": 119.99}', 3)
                                ON CONFLICT (""Type"") DO UPDATE
                                SET ""Price"" = 149.99, ""MultiCurrencyPrices"" = '{""ZAR"": 2749.99, ""EUR"": 139.99, ""GBP"": 119.99}';
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
                        // Find the Custom Pro package
                        var customPackage = await context.PricingPackages
                            .Include(p => p.Prices)
                            .FirstOrDefaultAsync(p => p.Type == "custom-pro");

                        if (customPackage != null)
                        {
                            Console.WriteLine($"Found Custom Pro package with current price: {customPackage.GetPrice()}");

                            // Update the prices using the new collection
                            customPackage.SetPrice(149.99m, "USD");
                            customPackage.SetPrice(2749.99m, "ZAR");
                            customPackage.SetPrice(139.99m, "EUR");
                            customPackage.SetPrice(119.99m, "GBP");

                            // Save changes
                            await context.SaveChangesAsync();
                            Console.WriteLine("Custom Pro package price updated successfully to 149.99.");
                        }
                        else
                        {
                            Console.WriteLine("Custom Pro package not found in the database. Trying to insert it...");

                            // Create a new Custom Pro package
                            var newCustomPackage = new PricingPackage
                            {
                                Title = "Custom Pro",
                                Description = "Flexible solutions tailored to unique requirements;Customizable features;Industry specific;Flexible scaling;Personalized onboarding;Custom workflows;Advanced integrations;Priority support",
                                Icon = "MUI:CustomIcon",
                                ExtraDescription = "Tailored solutions for unique business requirements",
                                TestPeriodDays = 14,
                                Type = "custom-pro",
                                TierLevel = 3
                            };

                            // Add and save the new package
                            context.PricingPackages.Add(newCustomPackage);
                            await context.SaveChangesAsync();
                            
                            // Now add the prices
                            newCustomPackage.SetPrice(149.99m, "USD");
                            newCustomPackage.SetPrice(2749.99m, "ZAR");
                            newCustomPackage.SetPrice(139.99m, "EUR");
                            newCustomPackage.SetPrice(119.99m, "GBP");
                            
                            await context.SaveChangesAsync();
                            Console.WriteLine("Custom Pro package created successfully with price 149.99.");
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
