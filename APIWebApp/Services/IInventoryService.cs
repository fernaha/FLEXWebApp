using APIWebApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIWebApp.Services
{
    public interface IInventoryService
    {
        Task EnsureInitializedAsync();
        Task<List<InventoryItem>> GetAllAsync();
        Task<InventoryItem?> GetByIdAsync(string id);
        Task AddOrUpdateAsync(InventoryItem item);
        Task DeleteAsync(string id);
    }
}
