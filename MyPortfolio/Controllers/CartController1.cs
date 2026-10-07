using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Data;
using MyPortfolio.Models;

namespace MyPortfolio.Controllers
{
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ILogger<CartController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Identity");
            }

            if (product.StockQuantity <= 0)
            {
                TempData["CartError"] = $"{product.Name} is currently out of stock.";
                return RedirectToAction("Productdetail", "Home", new { id = productId });
            }

            var cart = await _context.Carts.Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = user.Id
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = await _context.CartItems.FirstOrDefaultAsync(c => c.CartId == cart.Id && c.ProductId == productId);

            if (existingItem != null)
            {
                if (existingItem.Quantity >= product.StockQuantity)
                {
                    TempData["CartError"] = $"Only {product.StockQuantity} unit(s) of {product.Name} are available.";
                    return RedirectToAction("Cart");
                }
                existingItem.Quantity++;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = 1
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Cart");
        }

        [Authorize]

        public async Task<IActionResult> Cart()
        {      
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Identity");
            }

            var cart = await _context.Carts.Include(c => c.CartItems)
                .ThenInclude(c => c.Product).ThenInclude(p => p.Brand).Include(c => c.CartItems).ThenInclude(c => c.Product).ThenInclude(p => p.Category).FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = user.Id,
                    CartItems = new List<CartItem>()
                };
            }
                

            return View(cart);
        }

        [HttpPost]
        [Authorize]

        public async Task<IActionResult> RemoveCart(int id)
        {

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Identity");
            }
            var item = await _context.CartItems.Include(c => c.Cart).FirstOrDefaultAsync(c => c.Id == id && c.Cart.UserId == user.Id);
            if (item == null)
            
                return NotFound();

            _context.CartItems.Remove(item);

            await _context.SaveChangesAsync();

            return RedirectToAction("Cart");
            }
           
        

        [HttpPost]
        [Authorize]

        public async Task<IActionResult> IncreaseQ(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Identity");
            }
            var item = await _context.CartItems
                .Include(c => c.Cart)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id && c.Cart.UserId == user.Id);

            if (item == null)
            {
                return NotFound();
            }

            if (item.Quantity >= item.Product.StockQuantity)
            {
                TempData["CartError"] = $"Only {item.Product.StockQuantity} unit(s) of {item.Product.Name} are available.";
                return RedirectToAction("Cart");
            }

            item.Quantity++;
            await _context.SaveChangesAsync();

            return RedirectToAction("Cart");
        }

        [HttpPost]
        [Authorize]

        public async Task<IActionResult> DecreaseQ(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Identity");
            }
            var item = await _context.CartItems.Include(c => c.Cart).FirstOrDefaultAsync(c => c.Id == id && c.Cart.UserId == user.Id);

            if (item == null)
            
                return NotFound();

            if (item.Quantity > 1)
            {
                item.Quantity--;
            
            }else
            {
                _context.CartItems.Remove(item);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Cart");
        }
    }
}
