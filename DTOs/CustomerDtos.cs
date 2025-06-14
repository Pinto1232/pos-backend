using System.ComponentModel.DataAnnotations;

namespace PosBackend.DTOs
{
    // Lightweight DTO for listing customers
    public class CustomerListDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal LoyaltyPoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastVisit { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    // Detailed DTO for single customer view
    public class CustomerDetailDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public decimal LoyaltyPoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastVisit { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<CustomerGroupSummaryDto> CustomerGroups { get; set; } = new();
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
    }

    // Create/Update DTO
    public class CustomerCreateDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }
    }

    public class CustomerUpdateDto : CustomerCreateDto
    {
        public int CustomerId { get; set; }
    }

    // Simple DTO for dropdowns and references
    public class CustomerSummaryDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class CustomerGroupSummaryDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }
}