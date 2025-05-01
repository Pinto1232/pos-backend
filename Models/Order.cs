using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PosBackend.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [JsonIgnore] // Add this to prevent circular references
        public Customer? Customer { get; set; } // Make nullable

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("Store")]
        public int StoreId { get; set; }

        [JsonIgnore] // Add this to prevent circular references
        public Store? Store { get; set; } // Make nullable

        public DateTime OrderDate { get; set; }

        public required string Status { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        public string? ShippingAddress { get; set; }

        public string? Notes { get; set; } // Optional

        // Navigation property for items in the order
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
