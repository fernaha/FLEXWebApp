using APIWebApp.Models;
using APIWebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace APIWebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Username and password are required.");
                return View();
            }

            if (!await _userService.ValidateCredentialsAsync(username, password))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View();
            }

            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("PermissionLevel", user.PermissionLevel.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("ManageUsers");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Policy = "MinPermission3")]
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userService.GetAllAsync();
            return View(users);
        }

        [Authorize(Policy = "MinPermission3")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(string username, int permissionLevel, string? password)
        {
            if (string.IsNullOrWhiteSpace(username)) return BadRequest();
            var user = await _userService.GetByUsernameAsync(username) ?? new User { Username = username };
            user.PermissionLevel = permissionLevel;
            await _userService.AddOrUpdateAsync(user, string.IsNullOrWhiteSpace(password) ? null : password);
            return RedirectToAction("ManageUsers");
        }

        [Authorize(Policy = "MinPermission3")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return BadRequest();
            await _userService.DeleteAsync(username);
            return RedirectToAction("ManageUsers");
        }
    }
}
