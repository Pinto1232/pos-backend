using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class Store
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StoreId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        public virtual ICollection<Terminal> Terminals { get; set; } = new List<Terminal>();

    }
}
