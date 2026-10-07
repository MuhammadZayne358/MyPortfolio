using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MyPortfolio.Models;

public class CheckoutVM
{
    [ValidateNever]

    public Cart Cart { get; set; }

    public string FullName { get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public string City { get; set; }

    public string Address { get; set; }

    public string? PostalCode { get; set; }

    public string PaymentMethod { get; set; }
}