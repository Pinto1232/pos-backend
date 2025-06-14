using System.ComponentModel.DataAnnotations;

namespace PosBackend.Models
{
    public class Currency
    {
        [Key]
        [StringLength(3)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string Symbol { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public int DecimalPlaces { get; set; } = 2;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Keep for backward compatibility, but will be deprecated
        [Obsolete("Use ExchangeRate table instead")]
        public decimal ExchangeRate { get; set; }
    }
    
    public class ExchangeRate
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(3)]
        public string FromCurrency { get; set; } = string.Empty;
        
        [Required]
        [StringLength(3)]
        public string ToCurrency { get; set; } = string.Empty;
        
        public decimal Rate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ExpiresAt { get; set; }
    }
    
    public class CurrencyConversionResult
    {
        public decimal OriginalAmount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public decimal ConvertedAmount { get; set; }
        public string ToCurrency { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public DateTime ConvertedAt { get; set; }
    }
}
