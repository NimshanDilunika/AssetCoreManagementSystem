using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetsEmployee.Models
{
    [Table("AppNotification")]
    public class AppNotification
    {
        [Key]
        public int Id { get; set; }

        public string? TargetRole { get; set; }

        // Direct numeric ID link — no string or username comparison
        public int? EmployeeId { get; set; }

        public int? RequestId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsCleared { get; set; } = false;
    }
}