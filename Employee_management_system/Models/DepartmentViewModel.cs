using System.ComponentModel.DataAnnotations;

namespace Employee_management_system.Models
{
    public class DepartmentViewModel
    {
        public int Id { get; set; }

        [Required]
        public string DepartmentName { get; set; }
    }
}
