using AssetsEmployee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssetsEmployee.Controllers
{
    [Authorize(Roles = "OfficeUser,Admin,ITTechnician")]
    public class AssetRequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssetRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AssetRequest
        public IActionResult Index()
        {
            
            var requests = _context.AssetRequests
                .Include(r => r.Asset)
                .Include(r => r.Employee)
                .OrderByDescending(r => r.RequestDate)
                .ToList();

            return View(requests);
        }

        // GET: /AssetRequest/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Employees = new SelectList(_context.Employee, "EmployeeId", "EmployeeName");

            // Extract only unique category names (e.g., "Laptop", "Headset", "Monitor")
            var categories = _context.Asset
                .Select(a => a.AssetName.Trim())
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.Categories = new SelectList(categories);
            return View();
        }

        // POST: /AssetRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetRequest request)
        {
            if (ModelState.IsValid)
            {
                request.Status = RequestStatus.Pending;
                request.RequestDate = DateTime.UtcNow;

                _context.AssetRequests.Add(request);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Employees = new SelectList(_context.Employee, "EmployeeId", "EmployeeName", request.EmployeeId);
            ViewBag.Categories = new SelectList(_context.Asset.Select(a => a.AssetName.Trim()).Distinct());
            return View(request);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,DepartmentManager,ITTechnician")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int requestId, RequestStatus status)
        {
            var req = await _context.AssetRequests.FindAsync(requestId);
            if (req == null) return NotFound();

            req.ActionedDate = DateTime.UtcNow;
            req.ActionedByUserId = User.FindFirstValue(ClaimTypes.Name) ?? "admin";

            if (status == RequestStatus.Approved)
            {
                // Automatically allocate the first available asset of that category
                var availableAsset = await _context.Asset
                    .FirstOrDefaultAsync(a => a.Status == AssetStatus.Available
                                           && a.AssetName.ToLower() == req.RequestedCategory.ToLower());

                if (availableAsset != null)
                {
                    availableAsset.Status = AssetStatus.Assigned;
                    req.AssetId = availableAsset.AssetId;
                    req.Status = RequestStatus.Fulfilled;

                    _context.EmployeeAsset.Add(new EmployeeAsset
                    {
                        EmployeeId = req.EmployeeId,
                        AssetId = availableAsset.AssetId
                    });

                    _context.AssetAssignmentLogs.Add(new AssetAssignmentLog
                    {
                        AssetId = availableAsset.AssetId,
                        EmployeeId = req.EmployeeId,
                        AssignedDate = DateTime.UtcNow,
                        Notes = $"Auto-fulfilled Request #{req.RequestId} ({req.RequestedCategory})"
                    });
                }
                else
                {
                    // If no stock exists, still approve without linking
                    req.Status = RequestStatus.Approved;
                }
            }
            else
            {
                req.Status = RequestStatus.Rejected;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}