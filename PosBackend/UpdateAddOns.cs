using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PosBackend.Data;
using PosBackend.Models;

namespace PosBackend
{
    public class UpdateAddOns
    {
        public static async Task Main(string[] args)
        {
            // Setup DI
            var services = new ServiceCollection();
            services.AddDbContext<PosDbContext>(options =>
                options.UseNpgsql("Host=localhost;Database=posdb;Username=postgres;Password=postgres"));

            var serviceProvider = services.BuildServiceProvider();

            // Get DbContext
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PosDbContext>();

            // Update Advanced Analytics add-on
            await UpdateAddOn(dbContext, "Advanced Analytics", 
                new[] { "Real-time dashboard", "Custom report builder", "Data visualization tools", "Export to Excel/PDF", "Trend analysis" },
                new[] { "Internet connection", "Modern browser" });

            // Update API Access add-on
            await UpdateAddOn(dbContext, "API Access", 
                new[] { "RESTful API endpoints", "OAuth authentication", "Rate limiting", "Webhook support", "Comprehensive documentation" },
                new[] { "Developer account", "API key" });

            // Update Custom Branding add-on
            await UpdateAddOn(dbContext, "Custom Branding", 
                new[] { "Custom logo", "Color scheme customization", "Custom domain", "Email templates", "Receipt customization" },
                new[] { "Logo in SVG/PNG format", "Brand guidelines" });

            // Update 24/7 Support add-on
            await UpdateAddOn(dbContext, "24/7 Support", 
                new[] { "Phone support", "Live chat", "Priority email", "Remote assistance", "Dedicated support agent" },
                new[] { "Active subscription" });

            // Update Data Migration add-on
            await UpdateAddOn(dbContext, "Data Migration", 
                new[] { "Data mapping", "Automated transfer", "Data validation", "Historical data import", "Scheduled migration" },
                new[] { "Source system access", "Data backup", "Migration schedule" });

            // Update Training Sessions add-on
            await UpdateAddOn(dbContext, "Training Sessions", 
                new[] { "Personalized training", "Video tutorials", "Training materials", "Hands-on exercises", "Certification" },
                new[] { "Training schedule", "Staff availability" });

            // Update Hardware Bundle add-on
            await UpdateAddOn(dbContext, "Hardware Bundle", 
                new[] { "Receipt printer", "Barcode scanner", "Cash drawer", "Card reader", "Touch screen display" },
                new[] { "Shipping address", "Power requirements" });

            // Update Advanced Security add-on
            await UpdateAddOn(dbContext, "Advanced Security", 
                new[] { "Two-factor authentication", "Role-based access", "Audit logs", "Data encryption", "Compliance reporting" },
                new[] { "Security policy", "User management" });

            // Update Multi-currency Support add-on
            await UpdateAddOn(dbContext, "Multi-currency Support", 
                new[] { "Multiple currency support", "Automatic exchange rates", "Currency conversion", "Regional tax compliance", "Multi-currency reporting" },
                new[] { "Internet connection for exchange rates" });

            // Update Accounting Integration add-on
            await UpdateAddOn(dbContext, "Accounting Integration", 
                new[] { "QuickBooks integration", "Xero integration", "Automated sync", "Financial reporting", "Tax calculation" },
                new[] { "Accounting software account", "API credentials" });

            Console.WriteLine("Add-ons updated successfully!");
        }

        private static async Task UpdateAddOn(PosDbContext dbContext, string name, string[] features, string[] dependencies)
        {
            var addOn = await dbContext.AddOns.FirstOrDefaultAsync(a => a.Name == name);
            if (addOn != null)
            {
                addOn.Features = System.Text.Json.JsonSerializer.Serialize(features);
                addOn.Dependencies = System.Text.Json.JsonSerializer.Serialize(dependencies);
                await dbContext.SaveChangesAsync();
                Console.WriteLine($"Updated {name} add-on");
            }
            else
            {
                Console.WriteLine($"Add-on {name} not found");
            }
        }
    }
}