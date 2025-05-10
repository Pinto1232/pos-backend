namespace PosBackend.Application.Dtos.Permissions
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public string? Description { get; set; }
        public required string Module { get; set; }
        public bool IsActive { get; set; }
    }
}
