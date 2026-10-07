using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MyPortfolio.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ReferenceNo { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }

        [ValidateNever]

        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Please select a brand")]

        public int? BrandId { get; set; }

        [Required(ErrorMessage = "Please select a category")]

        public int? CategoryId { get; set; }

        public int CaseSize { get; set; }

        public int StockQuantity { get; set; }

        [ValidateNever]
        public Brand Brand { get; set; }

        [ValidateNever]
        public Category Category { get; set; }
    }
}
