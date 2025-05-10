namespace PosBackend.Application.Dtos.Roles
{
    public class RoleDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
        public int UserCount { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
