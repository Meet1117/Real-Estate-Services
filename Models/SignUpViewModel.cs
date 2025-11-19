using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Real_Estate_Services.Models
{
    public class SignUpViewModel
    {
        [Key]
        public int Id { get; set; }

        //Username Validation
        [Required(ErrorMessage = "Please enter username")]
        [Remote(action: "UserNameIsExist", controller: "Users")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Username cannot contain spaces")]
        public string Username { get; set; }

        //Email Validation
        [Required(ErrorMessage = "Please enter email")]
        [RegularExpression(@"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$")]
        public string Email { get; set; }

        //Password Validation
        [Required(ErrorMessage = "Please enter Password")]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        public string Password { get; set; }

        //Confirm Password Validation
        [Required(ErrorMessage = "Please enter Confirm Password")]
        [Compare("Password", ErrorMessage = "The passwords do not match.")]
        [Display(Name = "Confirm Password")]

        //IsActive Validation
        public string ConfirmPassword { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
