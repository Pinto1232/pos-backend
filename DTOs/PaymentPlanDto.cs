namespace POS.DTOs
{
    public class PaymentPlanDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public string? DiscountLabel { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPopular { get; set; }
        public bool IsDefault { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string[] ApplicableRegions { get; set; } = new[] { "*" };
        public string[] ApplicableUserTypes { get; set; } = new[] { "*" };
        public string Currency { get; set; } = "USD";
    }

    public class PaymentPlansResponse
    {
        public List<PaymentPlanDto> Plans { get; set; } = new();
        public int? DefaultPlanId { get; set; }
        public string Currency { get; set; } = "USD";
        public int TotalCount { get; set; }
    }

    public class CreatePaymentPlanRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public string? DiscountLabel { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPopular { get; set; } = false;
        public bool IsDefault { get; set; } = false;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string[] ApplicableRegions { get; set; } = new[] { "*" };
        public string[] ApplicableUserTypes { get; set; } = new[] { "*" };
        public string Currency { get; set; } = "USD";
    }

    public class UpdatePaymentPlanRequest
    {
        public string? Name { get; set; }
        public string? Period { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string? DiscountLabel { get; set; }
        public string? Description { get; set; }
        public bool? IsPopular { get; set; }
        public bool? IsDefault { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string[]? ApplicableRegions { get; set; }
        public string[]? ApplicableUserTypes { get; set; }
        public bool? IsActive { get; set; }
    }
}
