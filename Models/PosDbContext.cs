using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using POS.Models;

namespace PosBackend.Models
{
    public class PosDbContext : IdentityDbContext<User, UserRole, int>
    {
        public PosDbContext(DbContextOptions<PosDbContext> options)
            : base(options)
        {
        }

        // Products and Categories
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductExpiry> ProductExpiries { get; set; }

        // Inventory
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }

        // Sales and Orders
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }

        // Customers
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerGroup> CustomerGroups { get; set; }
        public DbSet<CustomerGroupMember> CustomerGroupMembers { get; set; }
        public DbSet<CustomerFeedback> CustomerFeedbacks { get; set; }
        public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }

        // Discounts and Coupons
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Coupon> Coupons { get; set; }

        // Store and Terminals
        public DbSet<Store> Stores { get; set; }
        public DbSet<Terminal> Terminals { get; set; }

        // Suppliers
        public DbSet<Supplier> Suppliers { get; set; }

        // Pricing and Packages
        public DbSet<PricingPackage> PricingPackages { get; set; }
        public DbSet<PaymentPlan> PaymentPlans { get; set; }
        public DbSet<Feature> CoreFeatures { get; set; }
        public DbSet<AddOn> AddOns { get; set; }
        public DbSet<UsageBasedPricing> UsageBasedPricing { get; set; }
        public DbSet<CustomPackageSelectedFeature> CustomPackageSelectedFeatures { get; set; }
        public DbSet<CustomPackageSelectedAddOn> CustomPackageSelectedAddOns { get; set; }
        public DbSet<CustomPackageUsageBasedPricing> CustomPackageSelectedUsageBasedPricing { get; set; }

        // User Customization
        public DbSet<UserCustomization> UserCustomizations { get; set; }

        // Currency
        public DbSet<Currency> Currencies { get; set; }

        // User Login History
        public DbSet<UserLoginHistory> UserLoginHistories { get; set; }

        // User Subscriptions
        public DbSet<UserSubscription> UserSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure soft delete filter for entities that support it
            // Commented out until IsDeleted property is added to these models
            // modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
            // modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);
            // modelBuilder.Entity<Sale>().HasQueryFilter(e => !e.IsDeleted);

            // Configure relationships
            modelBuilder.Entity<Category>()
                .HasMany(c => c.ChildCategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductVariants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductExpiries)
                .WithOne(e => e.Product)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Sales)
                .WithOne(s => s.Customer)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.LoyaltyPoint)
                .WithOne(l => l.Customer)
                .HasForeignKey<LoyaltyPoint>(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sale>()
                .HasMany(s => s.SaleItems)
                .WithOne(i => i.Sale)
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Invoice)
                .WithOne(i => i.Sale)
                .HasForeignKey<Invoice>(i => i.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sale>()
                .HasMany(s => s.Payments)
                .WithOne(p => p.Sale)
                .HasForeignKey(p => p.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.Terminals)
                .WithOne(t => t.Store)
                .HasForeignKey(t => t.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Supplier>()
                .HasMany(s => s.Products)
                .WithOne(p => p.Supplier)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasMany(i => i.StockAlerts)
                .WithOne(a => a.Inventory)
                .HasForeignKey(a => a.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Discount>()
                .HasMany(d => d.Coupons)
                .WithOne(c => c.Discount)
                .HasForeignKey(c => c.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerGroup>()
                .HasMany(g => g.CustomerGroupMembers)
                .WithOne(m => m.CustomerGroup)
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.CustomerGroupMembers)
                .WithOne(m => m.Customer)
                .HasForeignKey(m => m.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure composite keys
            modelBuilder.Entity<CustomPackageSelectedFeature>()
                .HasKey(c => new { c.PricingPackageId, c.FeatureId });

            modelBuilder.Entity<CustomPackageSelectedAddOn>()
                .HasKey(c => new { c.PricingPackageId, c.AddOnId });

            modelBuilder.Entity<CustomPackageUsageBasedPricing>()
                .HasKey(c => new { c.PricingPackageId, c.UsageBasedPricingId });

            // Configure JSON columns for UserCustomization
            modelBuilder.Entity<UserCustomization>()
                .Property(u => u.RegionalSettingsJson)
                .HasColumnType("jsonb");

            modelBuilder.Entity<UserCustomization>()
                .Property(u => u.TaxSettingsJson)
                .HasColumnType("jsonb");

            modelBuilder.Entity<CustomerGroupMember>()
                .HasKey(c => new { c.GroupId, c.CustomerId });
        }
    }
}
