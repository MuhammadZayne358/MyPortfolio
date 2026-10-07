using System.ComponentModel.DataAnnotations;

public class RegisterViewmodel
{
    [Required(ErrorMessage = "First Name is required")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "First Name must be between 2 and 50 characters")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last Name is required")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "Last Name must be between 2 and 50 characters")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirm Password is required")]
    [DataType(DataType.Password)]
    [Compare("Password",
        ErrorMessage = "Password and Confirm Password do not match")]
    public string ConfirmPassword { get; set; }
}