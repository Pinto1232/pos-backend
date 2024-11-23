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
                    DescriptionList = new List<string> // Use DescriptionList for seeding
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
                    DescriptionList = new List<string> // Use DescriptionList for seeding
                    {
                        "Expand your business capabilities with advanced modules and features.",
                        "Designed for growing businesses looking to enhance their POS system."
                    },
                    Icon = "growth-icon.png",
                    ExtraDescription = "Ideal for businesses looking to scale and grow.",
                    Price = 59.99m,
                    TestPeriodDays = 14
                }
            );
        }
    }
}