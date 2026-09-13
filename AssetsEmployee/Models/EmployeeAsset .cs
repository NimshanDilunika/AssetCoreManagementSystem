using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetsEmployee.Models
{
    [Table("EmployeeAsset")]
    public class EmployeeAsset
    {
        [Key]
        public int Id { get; set; }

        public int AssetId { get; set; }

        [ForeignKey(nameof(AssetId))]
        public virtual Asset? Asset { get; set; }

        public int EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee? Employee { get; set; }

        public DateTime? AssignedDate { get; set; }

        public DateTime? UnAssignedDate { get; set; }
    }
}