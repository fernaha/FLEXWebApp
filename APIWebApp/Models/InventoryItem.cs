using System;

namespace APIWebApp.Models
{
    public class InventoryItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StockCount { get; set; }
        // Relative URL to image under wwwroot (e.g. /uploads/abc.jpg)
        public string? ImagePath { get; set; }
    }
}
