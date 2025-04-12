namespace PosBackend.Models
{
    public class UserCustomization
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string SidebarColor { get; set; }
        public required string LogoUrl { get; set; }
        public required string NavbarColor { get; set; }
    }
}
