using System.ComponentModel.DataAnnotations;

namespace BuyZaar.ViewModels
{
    public class SetupPasswordViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}