using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class Terminal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TerminalId { get; set; }

        [ForeignKey("Store")]
        public int StoreId { get; set; }
        public required Store Store { get; set; }

        [Required]
        [StringLength(50)]
        public required string Name { get; set; }

        [StringLength(20)]
        public required string Status { get; set; }

        // Navigation properties
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
