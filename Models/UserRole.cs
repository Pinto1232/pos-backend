using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace PosBackend.Models
{
    public class UserRole : IdentityRole<int>
    {
        public string Permissions { get; set; } = "[]";

        // Navigation properties
        public virtual ICollection<UserRoleMapping> UserRoles { get; set; } = new List<UserRoleMapping>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        [NotMapped]
        public List<string> PermissionList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Permissions))
                {
                    return new List<string>();
                }
                try
                {
                    return JsonSerializer.Deserialize<List<string>>(Permissions) ?? new List<string>();
                }
                catch (JsonException ex)
                {
                    Console.Error.WriteLine($"Error deserializing permissions for role '{this.Name}' (ID: {this.Id}): {ex.Message}");
                    return new List<string>();
                }
            }
            set
            {
                Permissions = JsonSerializer.Serialize(value ?? new List<string>());
            }
        }
    }
}
