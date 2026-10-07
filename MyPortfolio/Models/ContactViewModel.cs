using System.ComponentModel.DataAnnotations;

namespace MyPortfolio.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Please enter your name")]
        [StringLength(100, ErrorMessage = "Name can be at most 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(150, ErrorMessage = "Email can be at most 150 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a subject")]
        [StringLength(150, ErrorMessage = "Subject can be at most 150 characters")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please write your message")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters")]
        public string Message { get; set; } = string.Empty;
    }
}
