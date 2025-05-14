using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PosBackend.Models
{
    public class UserSubscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int PricingPackageId { get; set; }

        [ForeignKey("PricingPackageId")]
        public PricingPackage? Package { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public string EnabledFeaturesJson { get; set; } = "[]";

        [NotMapped]
        public List<string> EnabledFeatures
        {
            get => string.IsNullOrEmpty(EnabledFeaturesJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(EnabledFeaturesJson) ?? new List<string>();
            set => EnabledFeaturesJson = JsonSerializer.Serialize(value ?? new List<string>());
        }

        // Additional packages that the user has enabled beyond their main subscription
        public string AdditionalPackagesJson { get; set; } = "[]";

        [NotMapped]
        public List<int> AdditionalPackages
        {
            get => string.IsNullOrEmpty(AdditionalPackagesJson)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(AdditionalPackagesJson) ?? new List<int>();
            set => AdditionalPackagesJson = JsonSerializer.Serialize(value ?? new List<int>());
        }

        // Audit tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
