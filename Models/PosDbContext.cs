using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace PosBackend.Models
{
    public class PosDbContext : DbContext
    {
        public PosDbContext(DbContextOptions<PosDbContext> options) : base(options) { }

        public DbSet<PricingPackage>? PricingPackages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PricingPackage>().HasData(
                new PricingPackage
                {
                    Id = 1,
                    Title = "Starter",
                    DescriptionList = new List<string> 
                    {
                        "Select the essential modules and features for your business.",
                        "Ideal for small businesses or those new to POS systems."
                    },
                    Icon = "starter-icon.png",
                    ExtraDescription = "This package is perfect for startups and small businesses.",
                    Price = 29.99m,
                    TestPeriodDays = 14
                },
                new PricingPackage
                {
                    Id = 2,
                    Title = "Growth",
                    DescriptionList = new List<string> 
                    {
                        "Expand your business capabilities with advanced modules and features.",
                        "Designed for growing businesses looking to enhance their POS system."
                    },
                    Icon = "growth-icon.png",
                    ExtraDescription = "Ideal for businesses looking to scale and grow.",
                    Price = 59.99m,
                    TestPeriodDays = 14
                },
                new PricingPackage
                {
                    Id = 3,
                    Title = "Custom",
                    DescriptionList = new List<string> 
                    {
                        "Tailor-made solutions for your unique business needs.",
                        "Perfect for businesses requiring customized POS features."
                    },
                    Icon = "custom-icon.png",
                    ExtraDescription = "Get a POS system that fits your specific requirements.",
                    Price = 99.99m,
                    TestPeriodDays = 30
                },
                new PricingPackage
                {
                    Id = 4,
                    Title = "Enterprise",
                    DescriptionList = new List<string> 
                    {
                        "Comprehensive POS solutions for large enterprises.",
                        "Includes all advanced features and premium support."
                    },
                    Icon = "enterprise-icon.png",
                    ExtraDescription = "Ideal for large businesses with extensive POS needs.",
                    Price = 199.99m,
                    TestPeriodDays = 30
                },
                new PricingPackage
                {
                    Id = 5,
                    Title = "Premium",
                    DescriptionList = new List<string> 
                    {
                        "All-inclusive POS package with premium features.",
                        "Best for businesses looking for top-tier POS solutions."
                    },
                    Icon = "premium-icon.png",
                    ExtraDescription = "Experience the best POS system with all features included.",
                    Price = 299.99m,
                    TestPeriodDays = 30
                }
            );
        }
    }
}