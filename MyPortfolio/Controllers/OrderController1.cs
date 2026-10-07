using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Data;
using MyPortfolio.Models;
using System.Threading.Tasks;

namespace MyPortfolio.Controllers
{
    public class OrderController : Controller
    {
        private readonly ILogger<OrderController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ILogger<OrderController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;

        }
        [Authorize]
        [HttpGet]

        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(c => c.Product)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Cart", "Cart");

            var vm = new CheckoutVM
            {
                Cart = cart,
                Email = user.Email ?? string.Empty,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                PaymentMethod = "Cash On Delivery",
            };


            return View(vm);

        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutVM model)
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(c => c.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Cart", "Cart");
            }

            if (!ModelState.IsValid)
            {
                model.Cart = cart;
                return View(model);
            }

            // Verify stock before placing the order
            foreach (var item in cart.CartItems)
            {
                if (item.Product.StockQuantity < item.Quantity)
                {
                    ModelState.AddModelError(string.Empty, $"Only {item.Product.StockQuantity} unit(s) of {item.Product.Name} are available. Please update your cart.");
                    model.Cart = cart;
                    return View(model);
                }
            }

            var order = new Order
            {
                UserId = user.Id,
                Status = "Pending",
                OrderDate = DateTime.Now,
                TotalAmount = cart.CartItems.Sum(c => c.Product.Price * c.Quantity),
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                City = model.City,
                Address = model.Address,
                PostalCode = model.PostalCode,
                PaymentMethod = model.PaymentMethod,
                IsPaid = false
            };
            _context.Orders.Add(order);

            foreach (var item in cart.CartItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    Order = order,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price,
                });

                item.Product.StockQuantity -= item.Quantity;
            }

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("Myorder");
        }

        [Authorize]
        public async Task<IActionResult> Myorder()
        {
            var user = await _userManager.GetUserAsync(User);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(o => o.Product)
                .ThenInclude(p => p.Brand)

                .Include(o => o.OrderItems)
                .ThenInclude(o => o.Product)
                .ThenInclude(p => p.Category)



                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(); 

            return View(orders);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminOrder()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminOrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Brand)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var allowed = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (!allowed.Contains(status))
            {
                TempData["AdminError"] = "Please choose a valid status.";
                return RedirectToAction("AdminOrderDetail", new { id });
            }

            if (order.Status == "Cancelled" ||
                 
        order.Status == "Delivered")
            {
                TempData["AdminError"] = "A shipped, delivered, or cancelled order can't be changed.";
                return RedirectToAction("AdminOrderDetail", new { id });
            }

            if (status == "Cancelled")
            {
                foreach (var item in order.OrderItems)
                {
                    item.Product.StockQuantity += item.Quantity;
                }
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            TempData["AdminSuccess"] = $"Order #{order.Id} is now {status}.";
            return RedirectToAction("AdminOrderDetail", new { id });
        }


    }
}
