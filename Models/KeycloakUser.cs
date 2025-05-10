using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PosBackend.Models
{
    public class KeycloakUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }
        
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
        
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        
        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }
        
        [JsonPropertyName("createdTimestamp")]
        public long CreatedTimestamp { get; set; }
        
        [JsonPropertyName("attributes")]
        public Dictionary<string, List<string>>? Attributes { get; set; }
        
        [JsonPropertyName("disableableCredentialTypes")]
        public List<string>? DisableableCredentialTypes { get; set; }
        
        [JsonPropertyName("requiredActions")]
        public List<string>? RequiredActions { get; set; }
        
        [JsonPropertyName("notBefore")]
        public int NotBefore { get; set; }
        
        [JsonPropertyName("access")]
        public Dictionary<string, bool>? Access { get; set; }
    }
}
