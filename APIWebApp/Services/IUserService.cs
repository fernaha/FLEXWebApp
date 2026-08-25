using APIWebApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIWebApp.Services
{
    public interface IUserService
    {
        Task EnsureInitializedAsync();
        Task<List<User>> GetAllAsync();
        Task<User?> GetByUsernameAsync(string username);
        Task AddOrUpdateAsync(User user, string? password = null);
        Task<bool> ValidateCredentialsAsync(string username, string password);
        Task DeleteAsync(string username);
    }
}
