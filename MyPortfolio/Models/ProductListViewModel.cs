namespace MyPortfolio.Models
{
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public List<Brand> Brands { get; set; }

        //public int CurrentPage { get; set; } = 1;
        //public int TotalPages { get; set; }
        //public int TotalCount { get; set; }
    }
}
