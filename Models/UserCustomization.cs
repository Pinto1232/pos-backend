using System.Text.Json.Serialization;
using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PosBackend.Models
{
    public class TaxCategory
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Rate { get; set; }
        public required string Description { get; set; }
        public bool IsDefault { get; set; }
    }

    public class TaxSettings
    {
        public bool EnableTaxCalculation { get; set; }
        public decimal DefaultTaxRate { get; set; }
        public string TaxCalculationMethod { get; set; } = "exclusive";
        public bool VatRegistered { get; set; }
        public string VatNumber { get; set; } = string.Empty;
        public bool EnableMultipleTaxRates { get; set; }
        public string TaxCategoriesJson { get; set; } = "[]";
        public bool DisplayTaxOnReceipts { get; set; }
        public bool EnableTaxExemptions { get; set; }
        public string TaxReportingPeriod { get; set; } = "monthly";

        [NotMapped]
        public List<TaxCategory> TaxCategories
        {
            get => string.IsNullOrEmpty(TaxCategoriesJson)
                ? new List<TaxCategory>()
                : JsonSerializer.Deserialize<List<TaxCategory>>(TaxCategoriesJson) ?? new List<TaxCategory>();
            set => TaxCategoriesJson = JsonSerializer.Serialize(value);
        }
    }

    public class RegionalSettings
    {
        public string DefaultCurrency { get; set; } = "ZAR";
        public string DateFormat { get; set; } = "DD/MM/YYYY";
        public string TimeFormat { get; set; } = "24h";
        public string Timezone { get; set; } = "Africa/Johannesburg";
        public string NumberFormat { get; set; } = "#,###.##";
        public string Language { get; set; } = "en-ZA";
        public bool AutoDetectLocation { get; set; } = true;
        public bool EnableMultiCurrency { get; set; } = true;
        public string SupportedCurrenciesJson { get; set; } = "[\"ZAR\",\"USD\",\"EUR\",\"GBP\"]";

        [NotMapped]
        public List<string> SupportedCurrencies
        {
            get => string.IsNullOrEmpty(SupportedCurrenciesJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(SupportedCurrenciesJson) ?? new List<string>();
            set => SupportedCurrenciesJson = JsonSerializer.Serialize(value);
        }
    }

    public class UserCustomization
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string SidebarColor { get; set; }
        public required string LogoUrl { get; set; }
        public required string NavbarColor { get; set; }

        // Store settings as JSON strings in the database
        public string? TaxSettingsJson { get; set; }
        public string? RegionalSettingsJson { get; set; }

        [NotMapped]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TaxSettings? TaxSettings
        {
            get => string.IsNullOrEmpty(TaxSettingsJson)
                ? null
                : JsonSerializer.Deserialize<TaxSettings>(TaxSettingsJson);
            set => TaxSettingsJson = value == null ? null : JsonSerializer.Serialize(value);
        }

        [NotMapped]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RegionalSettings? RegionalSettings
        {
            get => string.IsNullOrEmpty(RegionalSettingsJson)
                ? null
                : JsonSerializer.Deserialize<RegionalSettings>(RegionalSettingsJson);
            set => RegionalSettingsJson = value == null ? null : JsonSerializer.Serialize(value);
        }
    }
}
