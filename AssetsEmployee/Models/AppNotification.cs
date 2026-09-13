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

        // Nullable if targeted to a role group (e.g., Admin/ITTechnician)
        public string? TargetRole { get; set; }

        // Target username for specific user alerts (e.g., office user username)
        public string? TargetUsername { get; set; }

        // Linked request ID to enable automatic deletion when reviewed
        public int? RequestId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsCleared { get; set; } = false;
    }
}