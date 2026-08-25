using System;

namespace APIWebApp.Models
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        // Stored password hash (SHA256 hex)
        public string PasswordHash { get; set; } = string.Empty;
        // Permission level: 1..3
        public int PermissionLevel { get; set; } = 1;
    }
}
