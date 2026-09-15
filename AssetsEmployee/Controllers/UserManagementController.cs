using AssetsEmployee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AssetsEmployee.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _hasher;

        // Valid system roles
        private static readonly List<string> SystemRoles = new()
        {
            "Admin",
            "ITTechnician",
            "Auditor",
            "OfficeUser",
            "DepartmentManager"
        };

        public UserManagementController(ApplicationDbContext context, IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // GET: /UserManagement
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderBy(u => u.Username)
                .ToListAsync();

            return View(users);
        }

        // GET: /UserManagement/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(SystemRoles);
            return View();
        }

        // POST: /UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var exists = await _context.Users.AnyAsync(u => u.Username.ToLower() == model.Username.Trim().ToLower());
                if (exists)
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    ViewBag.Roles = new SelectList(SystemRoles, model.Role);
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username.Trim(),
                    Role = model.Role,
                    FailedLoginAttempts = 0,
                    LockoutEnd = null
                };

                // Hash password using the same hasher as AccountController
                user.Password = _hasher.HashPassword(user, model.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(SystemRoles, model.Role);
            return View(model);
        }

        // GET: /UserManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role
            };

            ViewBag.Roles = new SelectList(SystemRoles, model.Role);
            return View(model);
        }

        // POST: /UserManagement/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null) return NotFound();

                // Ensure username isn't taken by someone else
                var usernameConflict = await _context.Users.AnyAsync(u => u.Username.ToLower() == model.Username.Trim().ToLower() && u.UserId != model.UserId);
                if (usernameConflict)
                {
                    ModelState.AddModelError("Username", "Username is already taken by another account.");
                    ViewBag.Roles = new SelectList(SystemRoles, model.Role);
                    return View(model);
                }

                user.Username = model.Username.Trim();
                user.Role = model.Role;

                // Optional password reset
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    user.Password = _hasher.HashPassword(user, model.NewPassword);
                }

                // Unlock account if selected
                if (model.ResetLockout)
                {
                    user.FailedLoginAttempts = 0;
                    user.LockoutEnd = null;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(SystemRoles, model.Role);
            return View(model);
        }
        // GET: /UserManagement/ConfirmDeleteUser?userid=5
        [HttpGet]
        public async Task<IActionResult> ConfirmDeleteUser(int userid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userid);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /UserManagement/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int userid)
        {
            var user = await _context.Users.FindAsync(userid);
            if (user != null)
            {
                // Prevent deleting currently logged-in account
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId == user.UserId.ToString())
                {
                    TempData["Error"] = "You cannot delete your own logged-in account.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}