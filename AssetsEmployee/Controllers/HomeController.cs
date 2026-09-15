using AssetsEmployee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

namespace AssetsEmployee.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new DashboardViewModel();

            // Total counts
            model.TotalAssets = _context.Asset.Count();
            model.AssignedAssets = _context.EmployeeAsset.Count(ea => ea.UnAssignedDate == null);
            model.AvailableAssets = _context.Asset.Count(a => a.Status == AssetStatus.Available);
            model.PendingRequestsCount = _context.AssetRequests.Count(r => r.Status == RequestStatus.Pending);

            // Donut Chart data: Asset Statuses
            model.StatusAvailableCount = model.AvailableAssets;
            model.StatusAssignedCount = model.AssignedAssets;
            model.StatusOtherCount = _context.Asset.Count(a => a.Status != AssetStatus.Available && a.Status != AssetStatus.Assigned);

            // Bar Chart data: Group by Asset Category / Name
            var categoryStats = _context.Asset
                .GroupBy(a => a.AssetName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(6)
                .ToList();

            model.CategoryNames = categoryStats.Select(c => c.Name).ToList();
            model.CategoryCounts = categoryStats.Select(c => c.Count).ToList();

            // Read-only request summary
            model.RecentRequests = _context.AssetRequests
                .Include(r => r.Employee)
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .ToList();

            return View(model);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}