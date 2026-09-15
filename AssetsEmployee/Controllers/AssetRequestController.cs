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

                // 1. Fetch Employee name for notification message
                var employee = await _context.Employee.FindAsync(request.EmployeeId);
                string employeeName = employee?.EmployeeName ?? "Unknown Employee";

                // 2. Generate notification for Admin & ITTechnician
                var adminTechNotification = new AppNotification
                {
                    TargetRole = "AdminTech",
                    RequestId = request.RequestId,
                    Message = $"New Request: {employeeName} requested {request.RequestedCategory}. Reason: {request.Reason}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.AppNotifications.Add(adminTechNotification);
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
            var req = await _context.AssetRequests
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (req == null) return NotFound();

            req.ActionedDate = DateTime.UtcNow;
            req.ActionedByUserId = User.Identity?.Name ?? "admin";

            string assetName = string.IsNullOrWhiteSpace(req.RequestedCategory) ? "equipment" : req.RequestedCategory;

            if (status == RequestStatus.Approved)
            {
                var availableAsset = await _context.Asset
                    .FirstOrDefaultAsync(a => a.Status == AssetStatus.Available
                                           && a.AssetName.ToLower() == req.RequestedCategory.ToLower());

                if (availableAsset != null)
                {
                    availableAsset.Status = AssetStatus.Assigned;
                    req.AssetId = availableAsset.AssetId;
                    req.Status = RequestStatus.Fulfilled;
                    assetName = availableAsset.AssetName;

                    _context.EmployeeAsset.Add(new EmployeeAsset
                    {
                        EmployeeId = req.EmployeeId,
                        AssetId = availableAsset.AssetId,
                        AssignedDate = DateTime.UtcNow
                    });
                }
                else
                {
                    req.Status = RequestStatus.Approved;
                }

                // Office User Notification linked strictly by EmployeeId
                _context.AppNotifications.Add(new AppNotification
                {
                    TargetRole = "OfficeUser",
                    EmployeeId = req.EmployeeId,
                    RequestId = req.RequestId,
                    Message = $"Your request for {assetName} is accepted. Contact the department.",
                    CreatedAt = DateTime.UtcNow,
                    IsCleared = false
                });
            }
            else
            {
                req.Status = RequestStatus.Rejected;

                // Office User Notification linked strictly by EmployeeId
                _context.AppNotifications.Add(new AppNotification
                {
                    TargetRole = "OfficeUser",
                    EmployeeId = req.EmployeeId,
                    RequestId = req.RequestId,
                    Message = $"Your request for {assetName} is rejected. Contact the department.",
                    CreatedAt = DateTime.UtcNow,
                    IsCleared = false
                });
            }

            // Auto-delete Admin/Tech notification for this request
            var staffAlerts = await _context.AppNotifications
                .Where(n => n.RequestId == requestId && n.TargetRole == "AdminTech")
                .ToListAsync();

            if (staffAlerts.Any())
            {
                _context.AppNotifications.RemoveRange(staffAlerts);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}