using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Text.Json;

namespace PosBackend.Data
{
    public static class PackageTierSeeder
    {
        public static async Task SeedPackageTiers(PosDbContext context)
        {
            // Check if tiers already exist
            if (await context.PackageTiers.AnyAsync())
            {
                Console.WriteLine("Package tiers already exist in the database.");
                return;
            }

            var tiers = new List<PackageTier>
            {
                new PackageTier
                {
                    Name = "Starter Plus",
                    Description = "Essential features for small businesses starting their POS journey",
                    Level = 1,
                    MinPrice = 0,
                    MaxPrice = 50,
                    IsActive = true,
                    EnabledFeaturesJson = JsonSerializer.Serialize(new[]
                    {
                        "basic_pos",
                        "inventory_management",
                        "single_store",
                        "email_support",
                        "basic_reporting",
                        "customer_database",
                        "simple_analytics"
                    }),
                    RestrictedFeaturesJson = JsonSerializer.Serialize(new[]
                    {
                        "multi_store",
                        "advanced_analytics",
                        "api_access",
                        "white_label",
                        "custom_integrations"
                    }),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PackageTier
                {
                    Name = "Growth Pro",
                    Description = "Advanced features for growing businesses with expanding needs",
                    Level = 2,
                    MinPrice = 51,
                    MaxPrice = 100,
                    IsActive = true,
                    EnabledFeaturesJson = JsonSerializer.Serialize(new[]
                    {
                        "everything_in_starter",
                        "advanced_inventory",
                        "loyalty_program",
                        "marketing_automation",
                        "staff_tracking",
                        "custom_dashboards",
                        "mobile_app",
                        "multi_store_basic"
                    }),
                    RestrictedFeaturesJson = JsonSerializer.Serialize(new[]
                    {
                        "enterprise_analytics",
                        "white_label",
                        "dedicated_support",
                        "custom_api"
                    }),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PackageTier
                {
                    Name = "Custom Pro",
                    Description = "Flexible solutions tailored to unique business requirements",
                    Level = 3,
                    MinPrice = 101,
                    MaxPrice = 200,
                    IsActive = true,
                    EnabledFeaturesJson = JsonSerializer.Serialize(new[]
                    {
                        "customizable_features",
                        "industry_specific",
                        "flexible_scaling",
                        "personalized_onboarding",
                        "custom_workflows",
                        "advanced_integrations",
                        "priority_support"
                    }),
                    RestrictedFeaturesJson = JsonSerializer.Serialize(new[]
                    {
                        "enterprise_sla",
                        "dedicated_account_manager",
                        "white_label_full"
                    }),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PackageTier
                {
                    Name = "Enterprise Elite",
                    Description = "Comprehensive solutions for large organizations with complex needs",
                    Level = 4,
                    MinPrice = 201,
                    MaxPrice = 300,
                    IsActive = true,
                    EnabledFeaturesJson = JsonSerializer.Serialize(new[]
                    {
                        "all_features",
                        "multi_location_management",
                        "enterprise_analytics",
                        "custom_api_integrations",
                        "white_label_options",
                        "dedicated_account_manager",
                        "priority_24_7_support",
                        "advanced_security",
                        "compliance_tools"
                    }),
                    RestrictedFeaturesJson = JsonSerializer.Serialize(new string[] { }),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PackageTier
                {
                    Name = "Premium Plus",
                    Description = "Ultimate POS experience with cutting-edge AI and premium services",
                    Level = 5,
                    MinPrice = 301,
                    MaxPrice = 500,
                    IsActive = true,
                    EnabledFeaturesJson = JsonSerializer.Serialize(new[]
                    {
                        "everything_in_enterprise",
                        "ai_powered_analytics",
                        "predictive_inventory",
                        "omnichannel_integration",
                        "vip_support",
                        "quarterly_reviews",
                        "custom_reporting",
                        "advanced_ai_insights",
                        "machine_learning",
                        "premium_integrations"
                    }),
                    RestrictedFeaturesJson = JsonSerializer.Serialize(new string[] { }),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.PackageTiers.AddRangeAsync(tiers);
            await context.SaveChangesAsync();

            Console.WriteLine($"Successfully seeded {tiers.Count} package tiers.");
        }

        public static async Task UpdatePricingPackagesWithTiers(PosDbContext context)
        {
            var packages = await context.PricingPackages.ToListAsync();
            var tiers = await context.PackageTiers.ToListAsync();

            foreach (var package in packages)
            {
                var tier = package.Type?.ToLower() switch
                {
                    "starter-plus" => tiers.FirstOrDefault(t => t.Level == 1),
                    "growth-pro" => tiers.FirstOrDefault(t => t.Level == 2),
                    "custom-pro" => tiers.FirstOrDefault(t => t.Level == 3),
                    "enterprise-elite" => tiers.FirstOrDefault(t => t.Level == 4),
                    "premium-plus" => tiers.FirstOrDefault(t => t.Level == 5),
                    _ => null
                };

                if (tier != null)
                {
                    package.TierId = tier.Id;
                    package.TierLevel = tier.Level;
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"Updated {packages.Count} pricing packages with tier information.");
        }
    }
}