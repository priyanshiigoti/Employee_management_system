using Employee_management_system.Models.Entities;

namespace Employee_Management_System.Models
{
    public class ManagerDepartment
    {
        public int Id { get; set; }

        public string IdentityUserId { get; set; }
        public ApplicationUser IdentityUser { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; }
    }
}
