using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PosBackend.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PosBackend
{
    class UpdateCustomPackagePrice
    {
        // Renamed from Main to avoid entry point conflict
        public static async Task UpdatePackagePrice()
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
                    .Include(p => p.Prices)
                    .FirstOrDefaultAsync(p => p.Type == "custom");

                if (customPackage != null)
                {
                    // Update the prices using the new collection
                    customPackage.SetPrice(49.99m, "USD");
                    customPackage.SetPrice(899.99m, "ZAR");
                    customPackage.SetPrice(45.99m, "EUR");
                    customPackage.SetPrice(39.99m, "GBP");

                    // Save changes
                    await context.SaveChangesAsync();
                    Console.WriteLine("Custom package price updated successfully.");
                }
                else
                {
                    Console.WriteLine("Custom package not found in the database.");
                }
            }

            Console.WriteLine("Update completed.");
        }
    }
}
