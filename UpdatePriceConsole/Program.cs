using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PosBackend.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PosBackend.UpdatePriceConsole
{
    class Program
    {
        // Renamed from Main to avoid entry point conflict
        public static async Task UpdatePrice()
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

            // Create DbContext
            using (var context = new PosDbContext(optionsBuilder.Options))
            {
                Console.WriteLine("Updating Custom package price...");

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
                    Console.WriteLine("Custom package not found in the database.");
                }
            }

            Console.WriteLine("Update completed. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
