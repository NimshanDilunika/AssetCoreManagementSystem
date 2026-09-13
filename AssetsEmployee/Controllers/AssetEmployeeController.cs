using System;
using System.Linq;
using AssetsEmployee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetsEmployee.Controllers
{
    [Authorize(Roles = "Admin,ITTechnician,Auditor,OfficeUser")]
    public class EmployeeAssetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeAssetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard View
        [Authorize(Roles = "Admin,ITTechnician,Auditor,OfficeUser")]
        public IActionResult Index()
        {
            var assignments = _context.EmployeeAsset
                .Include(ea => ea.Asset)
                .Include(ea => ea.Employee)
                .OrderByDescending(ea => ea.AssignedDate)
                .ToList();

            return View(assignments);
        }
        // Add View
        [Authorize(Roles = "Admin,ITTechnician")]
        public IActionResult AddEmployeeAsset()
        {
            // Query the database by Status instead of the unmapped Available property
            ViewBag.Assets = _context.Asset
                .Where(a => a.Status == AssetStatus.Available)
                .ToList();

            ViewBag.Employees = _context.Employee.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ITTechnician")]
        public IActionResult AddEmployeeAssetItem(int assetid, int employeeid, DateTime? assigneddate)
        {
            if (assetid <= 0)
            {
                ModelState.AddModelError("AssetId", "Please select an asset.");
            }

            if (employeeid <= 0)
            {
                ModelState.AddModelError("EmployeeId", "Please select an employee.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Assets = _context.Asset
                    .Where(a => a.Status == AssetStatus.Available)
                    .ToList();

                ViewBag.Employees = _context.Employee.ToList();
                return View("AddEmployeeAsset");
            }

            var newAssignment = new EmployeeAsset
            {
                AssetId = assetid,
                EmployeeId = employeeid,
                AssignedDate = assigneddate ?? DateTime.Now,
                UnAssignedDate = null
            };

            _context.EmployeeAsset.Add(newAssignment);

            // Mark asset as Assigned
            var asset = _context.Asset.FirstOrDefault(a => a.AssetId == assetid);
            if (asset != null)
            {
                asset.Status = AssetStatus.Assigned;
                _context.Asset.Update(asset);
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Update View
        [Authorize(Roles = "Admin,ITTechnician")]
        public IActionResult UpdateEmployeeAsset(int id)
        {
            var item = _context.EmployeeAsset
                .Include(ae => ae.Asset)
                .FirstOrDefault(ea => ea.Id == id);
            if (item == null) return NotFound();

            ViewBag.Assets = _context.Asset
                .Where(a => a.AssetId == item.AssetId || a.Status == AssetStatus.Available)
                .ToList();

            ViewBag.Employees = _context.Employee.ToList();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ITTechnician")]
        public IActionResult UpdateEmployeeAssetItem(int id, int assetid, int employeeid, DateTime? assigneddate)
        {
            var item = _context.EmployeeAsset
                .Include(ae => ae.Asset)
                .FirstOrDefault(ea => ea.Id == id);
            if (item == null) return NotFound();

            if (item.AssetId != assetid)
            {
                // 1. Free up the old asset
                var oldAsset = _context.Asset.FirstOrDefault(a => a.AssetId == item.AssetId);
                if (oldAsset != null)
                {
                    oldAsset.Status = AssetStatus.Available;
                    _context.Asset.Update(oldAsset);
                }

                // 2. Mark the newly selected asset as Assigned
                var newAsset = _context.Asset.FirstOrDefault(a => a.AssetId == assetid);
                if (newAsset != null)
                {
                    newAsset.Status = AssetStatus.Assigned;
                    _context.Asset.Update(newAsset);
                }
            }

            item.AssetId = assetid;
            item.EmployeeId = employeeid;
            item.AssignedDate = assigneddate;

            _context.EmployeeAsset.Update(item);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Confirmation page for Unassign
        [Authorize(Roles = "Admin,ITTechnician")]
        public IActionResult ConfirmUnassign(int id)
        {
            var item = _context.EmployeeAsset
                .Include(ea => ea.Asset)
                .Include(ea => ea.Employee)
                .FirstOrDefault(ea => ea.Id == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // POST: Process Unassign
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ITTechnician")]
        public IActionResult UnassignAsset(int id)
        {
            var item = _context.EmployeeAsset
                .Include(ae => ae.Asset)
                .FirstOrDefault(ae => ae.Id == id);

            if (item != null)
            {
                item.UnAssignedDate = DateTime.Now;
                _context.EmployeeAsset.Update(item);

                if (item.Asset != null)
                {
                    item.Asset.Status = AssetStatus.Available;
                    _context.Asset.Update(item.Asset);
                }
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // GET: Confirmation page for Delete
        [Authorize(Roles = "Admin")]
        public IActionResult ConfirmDelete(int id)
        {
            var item = _context.EmployeeAsset
                .Include(ea => ea.Asset)
                .Include(ea => ea.Employee)
                .FirstOrDefault(ea => ea.Id == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // POST: Process Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteEmployeeAsset(int id)
        {
            var item = _context.EmployeeAsset
                .Include(ae => ae.Asset)
                .FirstOrDefault(ae => ae.Id == id);

            if (item != null)
            {
                if (item.Asset != null && item.UnAssignedDate == null)
                {
                    item.Asset.Status = AssetStatus.Available;
                    _context.Asset.Update(item.Asset);
                }
                _context.EmployeeAsset.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}