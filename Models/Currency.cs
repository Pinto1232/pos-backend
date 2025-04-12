using System.ComponentModel.DataAnnotations;

namespace PosBackend.Models
{
    public class Currency
    {
        [Key]
        public string Code { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
    }
}
