using AssetsEmployee.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AssetsEmployee.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _hasher;
        private const int MaxFailedAttempts = 5;
        private const int LockoutDurationMinutes = 15;

        public AccountController(ApplicationDbContext context, IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.Trim().ToLower());

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
                return View();
            }

            // 1. Verify Lockout Status
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var remaining = Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                ModelState.AddModelError(string.Empty, $"Account locked due to repeated failed logins. Try again in {remaining} minute(s).");
                return View();
            }

            // 2. Validate Password
            var result = _hasher.VerifyHashedPassword(user, user.Password, password);

            if (result == PasswordVerificationResult.Failed)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
                    user.FailedLoginAttempts = 0;
                    ModelState.AddModelError(string.Empty, $"Account locked for {LockoutDurationMinutes} minutes due to repeated failed logins.");
                }
                else
                {
                    int attemptsLeft = MaxFailedAttempts - user.FailedLoginAttempts;
                    ModelState.AddModelError(string.Empty, $"Invalid password. {attemptsLeft} attempt(s) remaining before lockout.");
                }

                await _context.SaveChangesAsync();
                return View();
            }

            // 3. Reset Lockout on Success
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            await _context.SaveChangesAsync();

            // 4. Issue Claims with Assigned Role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = false });

            // Respect direct return URLs if safe and local
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // 5. Intelligent Role Routing
            return user.Role switch
            {
                "Admin" => RedirectToAction("Index", "EmployeeAsset"),
                "ITTechnician" => RedirectToAction("Index", "Asset"),
                "Auditor" => RedirectToAction("Index", "Asset"),
                "OfficeUser" => RedirectToAction("Index", "Home"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}