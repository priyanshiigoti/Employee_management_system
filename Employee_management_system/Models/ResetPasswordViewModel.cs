using System.ComponentModel.DataAnnotations;

namespace Employee_management_system.Models
{
    public class ResetPasswordViewModel
    {
        [Required, EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required] public string Token { get; set; }

        [Required, DataType(DataType.Password)] public string Password { get; set; }

        [Compare("Password"), DataType(DataType.Password)] public string ConfirmPassword { get; set; }
    }

}
