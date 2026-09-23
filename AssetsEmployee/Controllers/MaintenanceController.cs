using AssetsEmployee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AssetsEmployee.Controllers
{
    [Authorize(Roles = "Admin,ITTechnician")]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Maintenance
        public async Task<IActionResult> Index()
        {
            var records = await _context.MaintenanceRecords
                .Include(m => m.Asset)
                .OrderByDescending(m => m.ScheduledDate)
                .ToListAsync();

            return View(records);
        }
        // Helper query to build the combined label
        private async Task PopulateAssetsDropdownAsync(int? selectedAssetId = null)
        {
            var assetList = await _context.Asset
                .OrderBy(a => a.AssetName)
                .Select(a => new
                {
                    a.AssetId,
                    DisplayText = a.SerialNo + " - " + a.AssetName
                })
                .ToListAsync();

            ViewBag.Assets = new SelectList(assetList, "AssetId", "DisplayText", selectedAssetId);
        }

        // GET: /Maintenance/AddMaintenance
        public async Task<IActionResult> AddMaintenance()
        {
            await PopulateAssetsDropdownAsync();
            return View();
        }

        // POST: /Maintenance/AddMaintenance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaintenance(MaintenanceRecord record)
        {
            if (ModelState.IsValid)
            {
                record.LoggedBy = User.Identity?.Name ?? "System";
                _context.MaintenanceRecords.Add(record);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateAssetsDropdownAsync(record.AssetId);
            return View(record);
        }

        // GET: /Maintenance/UpdateMaintenance/5
        public async Task<IActionResult> UpdateMaintenance(int id)
        {
            var record = await _context.MaintenanceRecords.FindAsync(id);
            if (record == null) return NotFound();

            await PopulateAssetsDropdownAsync(record.AssetId);
            return View(record);
        }

        // POST: /Maintenance/UpdateMaintenance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMaintenance(MaintenanceRecord record)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.MaintenanceRecords.FindAsync(record.MaintenanceId);
                if (existing == null) return NotFound();

                existing.AssetId = record.AssetId;
                existing.Title = record.Title;
                existing.Description = record.Description;
                existing.ServiceVendor = record.ServiceVendor;
                existing.Cost = record.Cost;
                existing.Status = record.Status;
                existing.ScheduledDate = record.ScheduledDate;
                existing.CompletedDate = record.CompletedDate;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateAssetsDropdownAsync(record.AssetId);
            return View(record);
        }

        // GET: /Maintenance/ConfirmDeleteMaintenance?id=5
        public async Task<IActionResult> ConfirmDeleteMaintenance(int id)
        {
            var record = await _context.MaintenanceRecords
                .Include(m => m.Asset)
                .FirstOrDefaultAsync(m => m.MaintenanceId == id);

            if (record == null) return NotFound();

            return View(record);
        }

        // POST: /Maintenance/DeleteMaintenance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMaintenance(int maintenanceid)
        {
            var record = await _context.MaintenanceRecords.FindAsync(maintenanceid);
            if (record != null)
            {
                _context.MaintenanceRecords.Remove(record);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}