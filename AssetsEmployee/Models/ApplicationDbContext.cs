using Microsoft.EntityFrameworkCore;

namespace AssetsEmployee.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Asset> Asset { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<EmployeeAsset> EmployeeAsset { get; set; }
        public DbSet<AssetRequest> AssetRequests { get; set; }
        public DbSet<AssetAssignmentLog> AssetAssignmentLogs { get; set; }
        public DbSet<AppNotification> AppNotifications { get; set; }

        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }

    }
}