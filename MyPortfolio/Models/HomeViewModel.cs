namespace MyPortfolio.Models
{
    public class CollectionSummary
    {
        public string Name { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }

    public class HomeViewModel
    {
        public List<Product> FeaturedProducts { get; set; } = new();
        public List<CollectionSummary> Collections { get; set; } = new();
        public List<string> BrandNames { get; set; } = new();
        public int ProductCount { get; set; }
        public int BrandCount { get; set; }
    }
}
