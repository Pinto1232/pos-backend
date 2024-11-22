namespace PosBackend.Models
{
    public class PricingPackage
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<string> Description { get; set; } = new List<string>();
        public string Icon { get; set; } = string.Empty;
        public string ExtraDescription { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TestPeriodDays { get; set; }
    }

}