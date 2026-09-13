using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetsEmployee.Models
{
    [Table("Department")]   
    
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        public string? Location{ get; set; }
        public string? Description { get; set; }

    }
}
