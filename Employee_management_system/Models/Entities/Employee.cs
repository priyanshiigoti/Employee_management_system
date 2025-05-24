using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Employee_management_system.Models.Entities
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string Phone { get; set; }

        public bool IsActive { get; set; } = true;

        // FK to Department
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        // FK to IdentityUser
        [Required]
        public string IdentityUserId { get; set; }
        public ApplicationUser IdentityUser { get; set; }
    }
}
