using APIWebApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace APIWebApp.Services
{
    public class FileInventoryService : IInventoryService
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private List<InventoryItem> _items = new();

        public FileInventoryService()
        {
            var dataDir = Path.Combine(AppContext.BaseDirectory, "App_Data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "items.json");
        }

        public async Task EnsureInitializedAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                {
                    _items = new List<InventoryItem>();
                    await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(_items, new JsonSerializerOptions { WriteIndented = true }));
                }
                else
                {
                    var json = await File.ReadAllTextAsync(_filePath);
                    _items = JsonSerializer.Deserialize<List<InventoryItem>>(json) ?? new List<InventoryItem>();
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<List<InventoryItem>> GetAllAsync()
        {
            await EnsureInitializedAsync();
            await _lock.WaitAsync();
            try
            {
                // Return copies
                return _items.Select(i => new InventoryItem { Id = i.Id, Name = i.Name, Description = i.Description, StockCount = i.StockCount, ImagePath = i.ImagePath }).ToList();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<InventoryItem?> GetByIdAsync(string id)
        {
            await EnsureInitializedAsync();
            await _lock.WaitAsync();
            try
            {
                var it = _items.FirstOrDefault(x => x.Id == id);
                if (it == null) return null;
                return new InventoryItem { Id = it.Id, Name = it.Name, Description = it.Description, StockCount = it.StockCount, ImagePath = it.ImagePath };
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task AddOrUpdateAsync(InventoryItem item)
        {
            await EnsureInitializedAsync();
            await _lock.WaitAsync();
            try
            {
                var existing = _items.FirstOrDefault(x => x.Id == item.Id);
                if (existing == null)
                {
                    _items.Add(item);
                }
                else
                {
                    existing.Name = item.Name;
                    existing.Description = item.Description;
                    existing.StockCount = item.StockCount;
                    existing.ImagePath = item.ImagePath;
                }
                await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(_items, new JsonSerializerOptions { WriteIndented = true }));
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task DeleteAsync(string id)
        {
            await EnsureInitializedAsync();
            await _lock.WaitAsync();
            try
            {
                _items.RemoveAll(x => x.Id == id);
                await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(_items, new JsonSerializerOptions { WriteIndented = true }));
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
