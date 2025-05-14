namespace PosBackend.Application.Services.Caching
{
    /// <summary>
    /// Constants for cache keys to ensure consistency across the application
    /// </summary>
    public static class CacheKeys
    {
        // User-related cache keys
        public static string User(int userId) => $"User:{userId}";
        public static string UserByUsername(string username) => $"User:Username:{username}";
        public static string UserByEmail(string email) => $"User:Email:{email}";
        public static string UserRoles(int userId) => $"User:{userId}:Roles";
        public static string AllUsers => "Users:All";
        
        // Package-related cache keys
        public static string Package(int packageId) => $"Package:{packageId}";
        public static string PackageByType(string type) => $"Package:Type:{type}";
        public static string AllPackages => "Packages:All";
        public static string PackageFeatures(string packageType) => $"Package:Features:{packageType}";
        public static string UserPackages(string userId) => $"User:{userId}:Packages";
        public static string UserFeatures(string userId) => $"User:{userId}:Features";
        
        // Product-related cache keys
        public static string Product(int productId) => $"Product:{productId}";
        public static string ProductsByCategory(int categoryId) => $"Products:Category:{categoryId}";
        public static string AllProducts => "Products:All";
        
        // Category-related cache keys
        public static string Category(int categoryId) => $"Category:{categoryId}";
        public static string AllCategories => "Categories:All";
        
        // Inventory-related cache keys
        public static string Inventory(int inventoryId) => $"Inventory:{inventoryId}";
        public static string InventoryByProduct(int productId) => $"Inventory:Product:{productId}";
        
        // Customer-related cache keys
        public static string Customer(int customerId) => $"Customer:{customerId}";
        public static string AllCustomers => "Customers:All";
        
        // GeoLocation-related cache keys
        public static string GeoLocation(string ipAddress) => $"GeoLocation:{ipAddress}";
        
        // Currency-related cache keys
        public static string Currency(string code) => $"Currency:{code}";
        public static string AllCurrencies => "Currencies:All";
        public static string ExchangeRate(string fromCode, string toCode) => $"Currency:ExchangeRate:{fromCode}:{toCode}";
        
        // Prefix constants for cache invalidation
        public static string UserPrefix => "User:";
        public static string PackagePrefix => "Package:";
        public static string ProductPrefix => "Product:";
        public static string CategoryPrefix => "Category:";
        public static string InventoryPrefix => "Inventory:";
        public static string CustomerPrefix => "Customer:";
        public static string GeoLocationPrefix => "GeoLocation:";
        public static string CurrencyPrefix => "Currency:";
    }
}
