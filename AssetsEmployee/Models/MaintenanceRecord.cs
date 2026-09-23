using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetsEmployee.Models
{
    [Table("MaintenanceRecords")]
    public class MaintenanceRecord
    {
        [Key]
        public int MaintenanceId { get; set; }

        [Required]
        public int AssetId { get; set; }

        [ForeignKey("AssetId")]
        public virtual Asset? Asset { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(150)]
        public string ServiceVendor { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; } = 0.00m;

        // Status options: Scheduled, InProgress, Completed, Cancelled
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";

        [Required]
        public DateTime ScheduledDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedDate { get; set; }

        [StringLength(100)]
        public string LoggedBy { get; set; } = string.Empty;
    }
}