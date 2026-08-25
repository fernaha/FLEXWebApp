using APIWebApp.Models;
using APIWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace APIWebApp.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventory;
        private readonly IWebHostEnvironment _env;

        public InventoryController(IInventoryService inventory, IWebHostEnvironment env)
        {
            _inventory = inventory;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _inventory.GetAllAsync();
            return View(items);
        }

        public async Task<IActionResult> Details(string id)
        {
            var item = await _inventory.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [Authorize(Policy = "MinPermission2")]
        public IActionResult Create()
        {
            return View(new InventoryItem());
        }

        [HttpPost]
        [Authorize(Policy = "MinPermission2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryItem model, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(model);

            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fs);
                }
                model.ImagePath = "/uploads/" + fileName;
            }

            await _inventory.AddOrUpdateAsync(model);
            return RedirectToAction("Index");
        }

        [Authorize(Policy = "MinPermission2")]
        public async Task<IActionResult> Edit(string id)
        {
            var item = await _inventory.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [Authorize(Policy = "MinPermission2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InventoryItem model, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(model);

            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fs);
                }
                model.ImagePath = "/uploads/" + fileName;
            }

            await _inventory.AddOrUpdateAsync(model);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Policy = "MinPermission2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            await _inventory.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
