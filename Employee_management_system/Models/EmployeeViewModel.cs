using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Employee_management_system.Models
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required."), EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]{3,}@[a-zA-Z0-9.-]{2,}\.(com|net|org)$", ErrorMessage = "Please enter a valid email address (e.g., john@example.com).")]
        public string Email { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string? Phone { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public string? DepartmentName { get; set; }

        public SelectList? Departments { get; set; }
    }
}
