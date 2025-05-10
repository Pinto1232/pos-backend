using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PosBackend.Application.Dtos.Users
{
    public class UserDetailDto
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Store the DateTime value internally
        [JsonIgnore]
        private DateTime? _lastLogin;

        // Public property that gets/sets the internal value
        [JsonIgnore]
        public DateTime? LastLogin
        {
            get => _lastLogin;
            set => _lastLogin = value;
        }

        // Explicitly serialize the lastLogin as a string in ISO format
        [JsonPropertyName("lastLogin")]
        public string? LastLoginString => _lastLogin?.ToString("o");

        // Additional property with a more human-readable format
        [JsonPropertyName("lastLoginFormatted")]
        public string? LastLoginFormatted => _lastLogin?.ToString("yyyy-MM-dd HH:mm:ss");

        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
