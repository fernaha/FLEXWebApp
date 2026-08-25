using APIWebApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace APIWebApp.Services
{
    public class FileUserService : IUserService
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private List<User> _users = new();

        public FileUserService()
        {
            var dataDir = Path.Combine(AppContext.BaseDirectory, "App_Data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "users.json");
        }

        public async Task EnsureInitializedAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                {
                    _users = new List<User>();
                    // Create a default admin user: admin / Password123 (permission 3)
                    var admin = new User
                    {
                        Username = "admin",
                        PermissionLevel = 3,
                        PasswordHash = ComputeHash("Password123")
                    };
                    _users.Add(admin);
                    await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true }));
                }
                else
                {
                    var json = await File.ReadAllTextAsync(_filePath);
                    _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<List<User>> GetAllAsync()
        {
            await EnsureInitializedAsync();
            await _lock.WaitAsync();
            try
            {
                return _users.Select(u => new User { Username = u.Username, PermissionLevel = u.PermissionLevel }).ToList();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            await EnsureInitializedAsync();
            await _lock.WaitAsync();
            try
            {
                var u = _users.FirstOrDefault(x => x.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
                if (u == null) return null;
                return new User { Username = u.Username, PermissionLevel = u.PermissionLevel, PasswordHash = u.PasswordHash };
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task AddOrUpdateAsync(User user, string? password = null)
        {
            await EnsureInitializedAsync();
            await _lock.WaitAsync();
            try
            {
                var existing = _users.FirstOrDefault(x => x.Username.Equals(user.Username, System.StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    existing = new User { Username = user.Username };
                    _users.Add(existing);
                }
                existing.PermissionLevel = user.PermissionLevel;
                if (!string.IsNullOrEmpty(password))
                {
                    existing.PasswordHash = ComputeHash(password);
                }
                await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true }));
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            await EnsureInitializedAsync();
            await _lock.WaitAsync();
            try
            {
                var user = _users.FirstOrDefault(x => x.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
                if (user == null) return false;
                var hash = ComputeHash(password);
                return string.Equals(hash, user.PasswordHash, System.StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task DeleteAsync(string username)
        {
            await EnsureInitializedAsync();
            await _lock.WaitAsync();
            try
            {
                _users.RemoveAll(x => x.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
                await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true }));
            }
            finally
            {
                _lock.Release();
            }
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
