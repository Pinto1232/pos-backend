using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class LoyaltyPoint : IEquatable<LoyaltyPoint>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoyaltyId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Points balance cannot be negative")]
        public int PointsBalance { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Audit and Soft Delete Tracking
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties
        public bool HasPoints => PointsBalance > 0;
        public bool CanRedeemPoints => PointsBalance >= 10; 

        // Total Points Tracking
        public int TotalPointsEarned { get; set; }
        public int TotalPointsRedeemed { get; set; }

        // Point Management Methods
        public void AddPoints(int points)
        {
            if (points > 0)
            {
                PointsBalance += points;
                TotalPointsEarned += points;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public bool RedeemPoints(int points)
        {
            if (points > 0 && PointsBalance >= points)
            {
                PointsBalance -= points;
                TotalPointsRedeemed += points;
                UpdatedAt = DateTime.UtcNow;
                return true;
            }
            return false;
        }


        public bool Equals(LoyaltyPoint? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return LoyaltyId == other.LoyaltyId &&
                   CustomerId == other.CustomerId;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LoyaltyPoint)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LoyaltyId, CustomerId);
        }
    }
}
