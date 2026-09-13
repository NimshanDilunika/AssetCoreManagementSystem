using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetsEmployee.Models
{
    [Table("AssetRequests")]
    public class AssetRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(100)]
        public string RequestedCategory { get; set; } = string.Empty;

        public int? AssetId { get; set; }

        [Required]
        [StringLength(250)]
        public string Reason { get; set; } = string.Empty;

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? ActionedDate { get; set; }
        public string? ActionedByUserId { get; set; }
        public string? ReviewerComments { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [ForeignKey("AssetId")]
        public virtual Asset? Asset { get; set; }
    }
}