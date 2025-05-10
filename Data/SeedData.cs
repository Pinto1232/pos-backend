using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;

namespace PosBackend.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PosDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Ensure database is created and migrated
                await context.Database.MigrateAsync();

                // Seed permissions
                await SeedPermissions(context);

                // Seed roles
                await SeedRoles(context, roleManager);

                // Seed admin user
                await SeedAdminUser(userManager, roleManager);

                // Seed role permissions
                await SeedRolePermissions(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedPermissions(PosDbContext context)
        {
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = new List<Permission>
                {
                    // User Management permissions
                    new Permission { Name = "View Users", Code = "users.view", Description = "Can view user list", Module = "User Management", IsActive = true },
                    new Permission { Name = "Create Users", Code = "users.create", Description = "Can create new users", Module = "User Management", IsActive = true },
                    new Permission { Name = "Edit Users", Code = "users.edit", Description = "Can edit existing users", Module = "User Management", IsActive = true },
                    new Permission { Name = "Delete Users", Code = "users.delete", Description = "Can delete users", Module = "User Management", IsActive = true },
                    
                    // Role Management permissions
                    new Permission { Name = "View Roles", Code = "roles.view", Description = "Can view role list", Module = "User Management", IsActive = true },
                    new Permission { Name = "Create Roles", Code = "roles.create", Description = "Can create new roles", Module = "User Management", IsActive = true },
                    new Permission { Name = "Edit Roles", Code = "roles.edit", Description = "Can edit existing roles", Module = "User Management", IsActive = true },
                    new Permission { Name = "Delete Roles", Code = "roles.delete", Description = "Can delete roles", Module = "User Management", IsActive = true },
                    
                    // Product Management permissions
                    new Permission { Name = "View Products", Code = "products.view", Description = "Can view product list", Module = "Product Management", IsActive = true },
                    new Permission { Name = "Create Products", Code = "products.create", Description = "Can create new products", Module = "Product Management", IsActive = true },
                    new Permission { Name = "Edit Products", Code = "products.edit", Description = "Can edit existing products", Module = "Product Management", IsActive = true },
                    new Permission { Name = "Delete Products", Code = "products.delete", Description = "Can delete products", Module = "Product Management", IsActive = true },
                    
                    // Sales permissions
                    new Permission { Name = "Create Sales", Code = "sales.create", Description = "Can create sales", Module = "Sales", IsActive = true },
                    new Permission { Name = "View Sales", Code = "sales.view", Description = "Can view sales", Module = "Sales", IsActive = true },
                    new Permission { Name = "Void Sales", Code = "sales.void", Description = "Can void sales", Module = "Sales", IsActive = true },
                };

                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedRoles(PosDbContext context, RoleManager<UserRole> roleManager)
        {
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<UserRole>
                {
                    new UserRole { Name = "Administrator", NormalizedName = "ADMINISTRATOR", ConcurrencyStamp = Guid.NewGuid().ToString() },
                    new UserRole { Name = "Manager", NormalizedName = "MANAGER", ConcurrencyStamp = Guid.NewGuid().ToString() },
                    new UserRole { Name = "Cashier", NormalizedName = "CASHIER", ConcurrencyStamp = Guid.NewGuid().ToString() }
                };

                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }
            }
        }

        private static async Task SeedAdminUser(UserManager<User> userManager, RoleManager<UserRole> roleManager)
        {
            if (await userManager.FindByNameAsync("admin") == null)
            {
                var user = new User
                {
                    UserName = "admin",
                    Email = "admin@pisvaltech.com",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Administrator");
                }
            }
        }

        private static async Task SeedRolePermissions(PosDbContext context)
        {
            // Check if role permissions are already seeded
            if (!await context.RolePermissions.AnyAsync())
            {
                // Get roles
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator");
                var managerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
                var cashierRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Cashier");

                // Get all permissions
                var permissions = await context.Permissions.ToListAsync();

                // Assign all permissions to admin role
                if (adminRole != null)
                {
                    foreach (var permission in permissions)
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = adminRole.Id,
                            PermissionId = permission.Id
                        });
                    }
                }

                // Assign specific permissions to manager role
                if (managerRole != null)
                {
                    var managerPermissions = permissions.Where(p => 
                        p.Code.StartsWith("products.") || 
                        p.Code.StartsWith("sales.") || 
                        p.Code == "users.view");

                    foreach (var permission in managerPermissions)
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = managerRole.Id,
                            PermissionId = permission.Id
                        });
                    }
                }

                // Assign specific permissions to cashier role
                if (cashierRole != null)
                {
                    var cashierPermissions = permissions.Where(p => 
                        p.Code == "products.view" || 
                        p.Code == "sales.create" || 
                        p.Code == "sales.view");

                    foreach (var permission in cashierPermissions)
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = cashierRole.Id,
                            PermissionId = permission.Id
                        });
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
