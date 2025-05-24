using Employee_management_system.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Employee_management_system.Models.Entities
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        public string DepartmentName { get; set; }

        // Navigation to Employees
        public ICollection<Employee> Employees { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }

    }
}
