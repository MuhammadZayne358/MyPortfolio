using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortfolio.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}