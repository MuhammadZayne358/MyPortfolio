using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortfolio.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }
    }
}