using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortfolio.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";
        public string FullName { get; set; }


        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Address { get; set; }

        public string? PostalCode { get; set; }

        // Payment
        [Required]
        public string PaymentMethod { get; set; }

        public bool IsPaid { get; set; } = false;

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}