using System.ComponentModel.DataAnnotations;

namespace Employee_management_system.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Email is required."), EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]{3,}@[a-zA-Z0-9.-]{2,}\.(com|net|org)$", ErrorMessage = "Please enter a valid email address (e.g., john@example.com).")]
        public string Email { get; set; }


        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string? Phone { get; set; }


        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

    }
}
