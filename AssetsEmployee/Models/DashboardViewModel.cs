using System.Collections.Generic;

namespace AssetsEmployee.Models
{
    public class DashboardViewModel
    {
        public int TotalAssets { get; set; }
        public int AssignedAssets { get; set; }
        public int AvailableAssets { get; set; }
        public int PendingRequestsCount { get; set; }

        // Graph data
        public List<string> CategoryNames { get; set; } = new();
        public List<int> CategoryCounts { get; set; } = new();

        public int StatusAvailableCount { get; set; }
        public int StatusAssignedCount { get; set; }
        public int StatusOtherCount { get; set; }

        // Read-only status list
        public List<AssetRequest> RecentRequests { get; set; } = new();
    }
}