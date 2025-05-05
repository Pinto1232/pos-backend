using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace PosBackend.Models
{
    public class PosDbContext : IdentityDbContext<User, UserRole, int>
    {
        public PosDbContext(DbContextOptions options) : base(options) { }

        public DbSet<PricingPackage> PricingPackages { get; set; }
        public DbSet<Feature> CoreFeatures { get; set; }
        public DbSet<AddOn> AddOns { get; set; }
        public DbSet<UsageBasedPricing> UsageBasedPricing { get; set; }
        public DbSet<Scope> Scopes { get; set; }

        public DbSet<CustomPackageSelectedFeature> CustomPackageSelectedFeatures { get; set; }
        public DbSet<CustomPackageSelectedAddOn> CustomPackageSelectedAddOns { get; set; }
        public DbSet<CustomPackageUsageBasedPricing> CustomPackageUsageBasedPricing { get; set; }

        // Dashboard customization
        public DbSet<UserCustomization> UserCustomizations { get; set; }

        // Currencies
        public DbSet<Currency> Currencies { get; set; }

        // User Management
        public DbSet<UserLoginHistory> UserLoginHistories { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Terminal> Terminals { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerGroup> CustomerGroups { get; set; }
        public DbSet<CustomerGroupMember> CustomerGroupMembers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductExpiry> ProductExpiries { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Scope entity
            modelBuilder.Entity<Scope>()
                .Property(s => s.Type)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            // Configure Role-UserRole relationship
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            // Pricing Package Configuration
            modelBuilder.Entity<PricingPackage>()
                .Property<string>("Currency")
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            modelBuilder.Entity<PricingPackage>()
                .Property<string>("MultiCurrencyPrices")
                .HasColumnType("text")
                .HasDefaultValue("{}");

            // Custom Package Relationships
            modelBuilder.Entity<CustomPackageSelectedFeature>()
                .HasKey(cf => new { cf.PricingPackageId, cf.FeatureId });

            modelBuilder.Entity<CustomPackageSelectedFeature>()
                .HasOne(cf => cf.Feature)
                .WithMany()
                .HasForeignKey(cf => cf.FeatureId);

            modelBuilder.Entity<CustomPackageSelectedAddOn>()
                .HasKey(ca => new { ca.PricingPackageId, ca.AddOnId });

            modelBuilder.Entity<CustomPackageSelectedAddOn>()
                .HasOne(ca => ca.AddOn)
                .WithMany()
                .HasForeignKey(ca => ca.AddOnId);

            modelBuilder.Entity<CustomPackageUsageBasedPricing>()
                .HasKey(cu => new { cu.PricingPackageId, cu.UsageBasedPricingId });

            modelBuilder.Entity<CustomPackageUsageBasedPricing>()
                .HasOne(cu => cu.UsageBasedPricing)
                .WithMany()
                .HasForeignKey(cu => cu.UsageBasedPricingId);

            // Currency Configuration
            modelBuilder.Entity<Currency>().HasKey(c => c.Code);
            modelBuilder.Entity<Currency>()
                .Property(c => c.ExchangeRate)
                .HasColumnType("decimal(18,4)");

            // Configure UserCustomization JSON columns
            modelBuilder.Entity<UserCustomization>()
                .Property(uc => uc.TaxSettingsJson)
                .HasColumnType("jsonb");

            modelBuilder.Entity<UserCustomization>()
                .Property(uc => uc.RegionalSettingsJson)
                .HasColumnType("jsonb");

            modelBuilder.Entity<PricingPackage>().HasData(
                new PricingPackage { Id = 1, Title = "Starter", Description = "Select the essential modules and features for your business.;Ideal for small businesses or those new to POS systems.", Icon = "MUI:StartIcon", ExtraDescription = "This package is perfect for startups and small businesses.", Price = 29.99m, TestPeriodDays = 14, Type = "starter" },
                new PricingPackage { Id = 2, Title = "Growth", Description = "Expand your business capabilities with advanced modules and features.;Designed for growing businesses looking to enhance their POS system.", Icon = "MUI:TrendingUpIcon", ExtraDescription = "Ideal for businesses looking to scale and grow.", Price = 59.99m, TestPeriodDays = 14, Type = "growth" },
                new PricingPackage { Id = 3, Title = "Custom", Description = "Tailor-made solutions for your unique business needs.;Perfect for businesses requiring customized POS features.", Icon = "MUI:BuildIcon", ExtraDescription = "Get a POS system that fits your specific requirements.", Price = 99.99m, TestPeriodDays = 30, Type = "custom" },
                new PricingPackage { Id = 4, Title = "Enterprise", Description = "Comprehensive POS solutions for large enterprises.;Includes all advanced features and premium support.", Icon = "MUI:BusinessIcon", ExtraDescription = "Ideal for large businesses with extensive POS needs.", Price = 199.99m, TestPeriodDays = 30, Type = "enterprise" },
                new PricingPackage { Id = 5, Title = "Premium", Description = "All-inclusive POS package with premium features.;Best for businesses looking for top-tier POS solutions.", Icon = "MUI:StarIcon", ExtraDescription = "Experience the best POS system with all features included.", Price = 299.99m, TestPeriodDays = 30, Type = "premium" }
            );

            modelBuilder.Entity<Feature>().HasData(
                new Feature { Id = 101, Name = "Inventory Management", Description = "Track and manage your inventory in real-time.", BasePrice = 10.00m, IsRequired = true },
                new Feature { Id = 102, Name = "Sales Reporting", Description = "Generate detailed reports on sales and revenue.", BasePrice = 8.00m, IsRequired = false },
                new Feature { Id = 103, Name = "Multi-Location Support", Description = "Manage multiple store locations from one dashboard.", BasePrice = 12.00m, IsRequired = false }
            );

            modelBuilder.Entity<AddOn>().HasData(
                new AddOn { Id = 201, Name = "Premium Support", Description = "24/7 priority support via chat and email.", Price = 5.00m },
                new AddOn { Id = 202, Name = "Custom Branding", Description = "Add your own logo and color scheme to the POS.", Price = 7.00m }
            );

            modelBuilder.Entity<UsageBasedPricing>().HasData(
                new UsageBasedPricing { Id = 1, FeatureId = 101, Name = "API Calls", Unit = "requests", MinValue = 1000, MaxValue = 100000, PricePerUnit = 0.01m },
                new UsageBasedPricing { Id = 2, FeatureId = 102, Name = "User Licenses", Unit = "users", MinValue = 1, MaxValue = 50, PricePerUnit = 5.00m }
            );

            modelBuilder.Entity<Currency>().HasData(
                new Currency { Code = "USD", ExchangeRate = 1.0m },
                new Currency { Code = "EUR", ExchangeRate = 0.9m },
                new Currency { Code = "GBP", ExchangeRate = 0.8m }
            );
        }
    }
}