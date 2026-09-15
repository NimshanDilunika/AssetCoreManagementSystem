using System.ComponentModel.DataAnnotations;

namespace AssetsEmployee.Models
{
    public class CreateUserViewModel
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "OfficeUser";
    }

    public class EditUserViewModel
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required]
        public string Role { get; set; } = "OfficeUser";

        public bool ResetLockout { get; set; } = false;
    }
}