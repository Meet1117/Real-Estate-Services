using System.ComponentModel.DataAnnotations;

namespace Real_Estate_Services.Models
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Please enter your username or email")]
        public string Identifier { get; set; }   // could be username or email

        [Required(ErrorMessage = "Please enter your current password")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Please enter a new password")]
        [DataType(DataType.Password)]
        [RegularExpression(
            "^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$",
            ErrorMessage = "Password must be at least 8 characters and contain 3 of: upper, lower, digit, special"
        )]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your new password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
