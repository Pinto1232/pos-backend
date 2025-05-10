using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PosBackend.Models
{
    public class PosDbContext : IdentityDbContext<User, UserRole, int>
    {
        public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerGroup> CustomerGroups { get; set; }
        public DbSet<CustomerGroupMember> CustomerGroupMembers { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Terminal> Terminals { get; set; }
        public DbSet<UserLoginHistory> UserLoginHistories { get; set; }
        public DbSet<UserCustomization> UserCustomizations { get; set; }
        public DbSet<PricingPackage> PricingPackages { get; set; }
        public DbSet<CoreFeature> CoreFeatures { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<AddOn> AddOns { get; set; }
        public DbSet<UsageBasedPricing> UsageBasedPricing { get; set; }
        public DbSet<CustomPackageSelectedFeature> CustomPackageSelectedFeatures { get; set; }

        // User & Role Management
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public new DbSet<UserRoleMapping> UserRoles { get; set; }
        public DbSet<CustomPackageSelectedAddOn> CustomPackageSelectedAddOns { get; set; }
        public DbSet<CustomPackageUsageBasedPricing> CustomPackageUsageBasedPricings { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Scope entity to use string for Type property
            modelBuilder.Entity<Scope>()
                .Property(s => s.Type)
                .HasConversion<string>();

            // Configure relationships
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.ProductVariants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Store)
                .WithMany()
                .HasForeignKey(i => i.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.ProductVariant)
                .WithMany()
                .HasForeignKey(i => i.VariantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockAlert>()
                .HasOne(sa => sa.Inventory)
                .WithMany(i => i.StockAlerts)
                .HasForeignKey(sa => sa.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Terminal>()
                .HasOne(t => t.Store)
                .WithMany(s => s.Terminals)
                .HasForeignKey(t => t.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Terminal)
                .WithMany(t => t.Sales)
                .HasForeignKey(s => s.TerminalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.ProductVariant)
                .WithMany()
                .HasForeignKey(si => si.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductVariant)
                .WithMany()
                .HasForeignKey(oi => oi.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoyaltyPoint>()
                .HasOne(lp => lp.Customer)
                .WithOne(c => c.LoyaltyPoint)
                .HasForeignKey<LoyaltyPoint>(lp => lp.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerGroupMember>()
                .HasKey(cgm => new { cgm.GroupId, cgm.CustomerId });

            modelBuilder.Entity<CustomerGroupMember>()
                .HasOne(cgm => cgm.CustomerGroup)
                .WithMany(cg => cg.CustomerGroupMembers)
                .HasForeignKey(cgm => cgm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerGroupMember>()
                .HasOne(cgm => cgm.Customer)
                .WithMany(c => c.CustomerGroupMembers)
                .HasForeignKey(cgm => cgm.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Coupon>()
                .HasOne(c => c.Discount)
                .WithMany(d => d.Coupons)
                .HasForeignKey(c => c.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomPackageSelectedFeature>()
                .HasKey(cpsf => new { cpsf.PricingPackageId, cpsf.FeatureId });

            modelBuilder.Entity<CustomPackageSelectedAddOn>()
                .HasKey(cpsa => new { cpsa.PricingPackageId, cpsa.AddOnId });

            // Seed data for PricingPackages
            modelBuilder.Entity<PricingPackage>().HasData(
                new PricingPackage
                {
                    Id = 1,
                    Title = "Starter",
                    Description = "Select the essential modules and features for your business.;Ideal for small businesses or those new to POS systems.",
                    Icon = "MUI:StartIcon",
                    ExtraDescription = "This package is perfect for startups and small businesses.",
                    Price = 29.99m,
                    TestPeriodDays = 14,
                    Type = "starter",
                    Currency = "",
                    MultiCurrencyPrices = "{}"
                },
                new PricingPackage
                {
                    Id = 2,
                    Title = "Growth",
                    Description = "Expand your business with advanced features.;Perfect for growing businesses with multiple products.",
                    Icon = "MUI:TrendingUpIcon",
                    ExtraDescription = "Scale your business with our growth package.",
                    Price = 59.99m,
                    TestPeriodDays = 14,
                    Type = "growth",
                    Currency = "",
                    MultiCurrencyPrices = "{}"
                },
                new PricingPackage
                {
                    Id = 3,
                    Title = "Custom",
                    Description = "Build your own package with the features you need.;Pay only for what your business requires.",
                    Icon = "MUI:BuildIcon",
                    ExtraDescription = "Customize your POS experience.",
                    Price = 39.99m,
                    TestPeriodDays = 14,
                    Type = "custom",
                    Currency = "",
                    MultiCurrencyPrices = "{}"
                },
                new PricingPackage
                {
                    Id = 4,
                    Title = "Enterprise",
                    Description = "Comprehensive POS solutions for large enterprises.;Includes all advanced features and premium support.",
                    Icon = "MUI:BusinessIcon",
                    ExtraDescription = "Ideal for large businesses with extensive POS needs.",
                    Price = 199.99m,
                    TestPeriodDays = 30,
                    Type = "enterprise",
                    Currency = "",
                    MultiCurrencyPrices = "{}"
                },
                new PricingPackage
                {
                    Id = 5,
                    Title = "Premium",
                    Description = "All-inclusive POS package with premium features.;Best for businesses looking for top-tier POS solutions.",
                    Icon = "MUI:StarIcon",
                    ExtraDescription = "Experience the best POS system with all features included.",
                    Price = 299.99m,
                    TestPeriodDays = 30,
                    Type = "premium",
                    Currency = "",
                    MultiCurrencyPrices = "{}"
                }
            );

            // Seed data for CoreFeatures
            modelBuilder.Entity<CoreFeature>().HasData(
                new CoreFeature
                {
                    Id = 101,
                    Name = "Inventory Management",
                    Description = "Track and manage your inventory in real-time.",
                    BasePrice = 10.00m,
                    IsRequired = true
                },
                new CoreFeature
                {
                    Id = 102,
                    Name = "Sales Reporting",
                    Description = "Generate detailed reports on sales and revenue.",
                    BasePrice = 8.00m,
                    IsRequired = false
                },
                new CoreFeature
                {
                    Id = 103,
                    Name = "Multi-Location Support",
                    Description = "Manage multiple store locations from one dashboard.",
                    BasePrice = 12.00m,
                    IsRequired = false
                }
            );

            // Seed data for AddOns
            modelBuilder.Entity<AddOn>().HasData(
                new AddOn
                {
                    Id = 201,
                    Name = "Premium Support",
                    Description = "24/7 priority support via chat and email.",
                    Price = 5.00m
                },
                new AddOn
                {
                    Id = 202,
                    Name = "Custom Branding",
                    Description = "Add your own logo and color scheme to the POS.",
                    Price = 7.00m
                }
            );

            // Seed data for Currencies
            modelBuilder.Entity<Currency>().HasData(
                new Currency
                {
                    Code = "USD",
                    ExchangeRate = 1.0m
                },
                new Currency
                {
                    Code = "EUR",
                    ExchangeRate = 0.9m
                },
                new Currency
                {
                    Code = "GBP",
                    ExchangeRate = 0.8m
                }
            );

            // Seed data for UsageBasedPricing
            modelBuilder.Entity<UsageBasedPricing>().HasData(
                new UsageBasedPricing
                {
                    Id = 1,
                    FeatureId = 101,
                    Name = "Number of Products",
                    Unit = "products",
                    MinValue = 100,
                    MaxValue = 10000,
                    PricePerUnit = 0.05m
                },
                new UsageBasedPricing
                {
                    Id = 2,
                    FeatureId = 103,
                    Name = "Number of Locations",
                    Unit = "locations",
                    MinValue = 1,
                    MaxValue = 50,
                    PricePerUnit = 5.00m
                }
            );
        }
    }
}
