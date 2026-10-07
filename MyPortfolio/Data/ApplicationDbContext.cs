using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Models;

namespace MyPortfolio.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Product> Products { get; set; }
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>().Property(o => o.Price).HasPrecision(18, 2);
        }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders{ get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<ContactMessage> ContactMessages { get; set; }

    }
}