namespace MyPortfolio.Models
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int UnreadMessages { get; set; }
        public int Year { get; set; }
        public decimal[] MonthlyRevenue { get; set; } = new decimal[12];
        public List<CollectionSummary> Categories { get; set; } = new();
        public List<Order> RecentOrders { get; set; } = new();
        public List<ContactMessage> RecentMessages { get; set; } = new();
    }

    public class AdminContactViewModel
    {
        public List<ContactMessage> Messages { get; set; } = new();
        public ContactMessage? Selected { get; set; }
        public int UnreadCount => Messages.Count(m => !m.IsRead);
    }
}
