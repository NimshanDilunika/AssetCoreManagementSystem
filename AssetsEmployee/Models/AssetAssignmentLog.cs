using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetsEmployee.Models
{
    [Table("AssetAssignmentLogs")]
    public class AssetAssignmentLog
    {
        [Key]
        public int LogId { get; set; }

        public int AssetId { get; set; }
        public int EmployeeId { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReturnedDate { get; set; }

        public string? Notes { get; set; }

        [ForeignKey("AssetId")]
        public virtual Asset? Asset { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
    }
}