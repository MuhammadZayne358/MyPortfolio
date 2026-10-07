using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MyPortfolio.Models
{
    public class ViewmodelProduct
    {
        public Product Product { get; set; }

        [ValidateNever]

        public List<Category> Categories { get; set; }

        [ValidateNever]

        public List<Brand> Brands { get; set; }
    }
}
