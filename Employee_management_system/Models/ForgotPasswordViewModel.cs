using System.ComponentModel.DataAnnotations;

namespace Employee_management_system.Models
{
    public class ForgotPasswordViewModel
    {

        [Required(ErrorMessage = "Email is required."), EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]{3,}@[a-zA-Z0-9.-]{2,}\.(com|net|org)$", ErrorMessage = "Please enter a valid email address (e.g., john@example.com).")]
        public string Email { get; set; }

    }
}
