using System.ComponentModel.DataAnnotations;

namespace MyPortfolio.Models
{
    public class ResetPassword
    {
        public string Email { get; set; }

        public string Token { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [DataType(DataType.Password)]

        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
