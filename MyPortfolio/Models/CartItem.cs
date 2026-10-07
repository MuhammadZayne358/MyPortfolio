using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortfolio.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        public int CartId { get; set; }

        [ForeignKey(nameof(CartId))]
        public Cart Cart { get; set; }

        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        public int Quantity { get; set; } = 1;
    }
}