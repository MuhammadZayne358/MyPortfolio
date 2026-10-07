using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Data;
using MyPortfolio.Models;
using System.Diagnostics;

namespace MyPortfolio.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".jfif" };
        private const long MaxImageBytes = 5 * 1024 * 1024;

        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _env = env;
        }

        // ───────────── Public pages ─────────────

        public IActionResult Index()
        {
            var vm = new HomeViewModel
            {
                FeaturedProducts = _context.Products
                    .AsNoTracking()
                    .AsNoTracking()
                    .Include(p => p.Brand)
                    .OrderByDescending(p => p.Id)
                    .Take(4)
                    .ToList(),
                Collections = _context.Categories
                    .AsNoTracking()
                    .Select(c => new CollectionSummary { Name = c.Name, ProductCount = c.Products.Count() })
                    .OrderByDescending(c => c.ProductCount)
                    .Take(4)
                    .ToList(),
                BrandNames = _context.Brands.AsNoTracking().OrderBy(b => b.Name).Select(b => b.Name).Take(8).ToList(),
                ProductCount = _context.Products.Count(),
                BrandCount = _context.Brands.Count()
            };

            return View(vm);
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.ContactMessages.Add(new ContactMessage
            {
                Name = model.Name.Trim(),
                Email = model.Email.Trim(),
                Subject = model.Subject.Trim(),
                Message = model.Message.Trim(),
                CreatedAt = DateTime.Now,
                IsRead = false
            });
            await _context.SaveChangesAsync();

            TempData["ContactSuccess"] = "Thank you! Your message has been sent. Our advisors will reply to you by email.";
            return RedirectToAction(nameof(Contact));
        }

        public IActionResult Product()
        {
            var vm = new ProductListViewModel
            {
                Products = _context.Products
                    .AsNoTracking()
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.Id)
                    .ToList(),
                Categories = _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToList(),
                Brands = _context.Brands.AsNoTracking().OrderBy(b => b.Name).ToList(),
            };

            return View(vm);
        }

        public IActionResult Productdetail(int id)
        {
            var product = _context.Products
                .AsNoTracking()
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // ───────────── Admin: products ─────────────

        public async Task<IActionResult> Adminashboard()
        {
            var year = DateTime.Now.Year;
            var activeOrders = _context.Orders.AsNoTracking().Where(o => o.Status != "Cancelled");

            var yearOrders = await activeOrders
                .Where(o => o.OrderDate.Year == year)
                .Select(o => new { o.OrderDate.Month, o.TotalAmount })
                .ToListAsync();

            var monthly = new decimal[12];
            foreach (var o in yearOrders)
            {
                monthly[o.Month - 1] += o.TotalAmount;
            }

            var vm = new AdminDashboardViewModel
            {
                Year = year,
                MonthlyRevenue = monthly,
                TotalRevenue = await activeOrders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                TotalOrders = await _context.Orders.CountAsync(),
                TotalCustomers = await _context.Users.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                LowStockCount = await _context.Products.CountAsync(p => p.StockQuantity <= 2),
                UnreadMessages = await _context.ContactMessages.CountAsync(m => !m.IsRead),
                Categories = await _context.Categories
                    .AsNoTracking()
                    .Select(c => new CollectionSummary { Name = c.Name, ProductCount = c.Products.Count() })
                    .OrderByDescending(c => c.ProductCount)
                    .Take(5)
                    .ToListAsync(),
                RecentOrders = await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync(),
                RecentMessages = await _context.ContactMessages
                    .AsNoTracking()
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(4)
                    .ToListAsync()
            };

            return View(vm);
        }

        public async Task<IActionResult> Adminproduct()
        {
            var vm = new ProductListViewModel
            {
                Products = await _context.Products
                    .AsNoTracking()
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.Id)
                    .ToListAsync(),
                Categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(),
                Brands = await _context.Brands.AsNoTracking().OrderBy(b => b.Name).ToListAsync()
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            var vm = new ViewmodelProduct { Product = new Product() };
            await LoadLookups(vm);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(ViewmodelProduct vm, IFormFile? imagefile)
        {
            if (ModelState.IsValid)
            {
                var (imageUrl, error) = await SaveImageAsync(imagefile);
                if (error != null)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                else
                {
                    vm.Product.ImageUrl = imageUrl ?? string.Empty;
                    _context.Products.Add(vm.Product);
                    await _context.SaveChangesAsync();

                    TempData["AdminSuccess"] = $"\"{vm.Product.Name}\" has been added to the collection.";
                    return RedirectToAction(nameof(Adminproduct));
                }
            }

            await LoadLookups(vm);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Editproduct(int id)
        {
            var prod = await _context.Products.FindAsync(id);
            if (prod == null)
            {
                return NotFound();
            }

            var vm = new ViewmodelProduct { Product = prod };
            await LoadLookups(vm);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Editproduct(ViewmodelProduct vm, IFormFile? imagefile, int id)
        {
            var prod = await _context.Products.FindAsync(id);
            if (prod == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var (imageUrl, error) = await SaveImageAsync(imagefile);
                if (error != null)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                else
                {
                    prod.Name = vm.Product.Name;
                    prod.ReferenceNo = vm.Product.ReferenceNo;
                    prod.Price = vm.Product.Price;
                    prod.Description = vm.Product.Description;
                    prod.BrandId = vm.Product.BrandId;
                    prod.CategoryId = vm.Product.CategoryId;
                    prod.CaseSize = vm.Product.CaseSize;
                    prod.StockQuantity = vm.Product.StockQuantity;

                    if (imageUrl != null)
                    {
                        prod.ImageUrl = imageUrl;
                    }

                    await _context.SaveChangesAsync();

                    TempData["AdminSuccess"] = $"\"{prod.Name}\" has been updated.";
                    return RedirectToAction(nameof(Adminproduct));
                }
            }

            vm.Product.Id = prod.Id;
            vm.Product.ImageUrl = prod.ImageUrl;
            await LoadLookups(vm);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var prod = await _context.Products.FindAsync(id);
            if (prod == null)
            {
                return NotFound();
            }

            return View(new ViewmodelProduct { Product = prod });
        }

        [HttpPost, ActionName("DeleteProduct")]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            var prod = await _context.Products.FindAsync(id);
            if (prod == null)
            {
                return NotFound();
            }

            var usedInOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
            if (usedInOrders)
            {
                TempData["AdminError"] = $"\"{prod.Name}\" is part of existing customer orders, so it can't be deleted. Set its stock to 0 instead.";
                return RedirectToAction(nameof(Adminproduct));
            }

            _context.Products.Remove(prod);
            await _context.SaveChangesAsync();

            TempData["AdminSuccess"] = $"\"{prod.Name}\" has been deleted.";
            return RedirectToAction(nameof(Adminproduct));
        }

        // ───────────── Admin: contact messages ─────────────

        public async Task<IActionResult> AdminContact(int? id)
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var vm = new AdminContactViewModel { Messages = messages };

            if (id.HasValue)
            {
                vm.Selected = messages.FirstOrDefault(m => m.Id == id.Value);
                if (vm.Selected != null && !vm.Selected.IsRead)
                {
                    vm.Selected.IsRead = true;
                    await _context.SaveChangesAsync();
                }
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleContactRead(int id)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg == null)
            {
                return NotFound();
            }

            msg.IsRead = !msg.IsRead;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AdminContact), msg.IsRead ? new { id } : null);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg != null)
            {
                _context.ContactMessages.Remove(msg);
                await _context.SaveChangesAsync();
                TempData["AdminSuccess"] = "Message deleted.";
            }
            return RedirectToAction(nameof(AdminContact));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllContactsRead()
        {
            var unread = await _context.ContactMessages.Where(m => !m.IsRead).ToListAsync();
            foreach (var m in unread)
            {
                m.IsRead = true;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AdminContact));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReadContacts()
        {
            var read = await _context.ContactMessages.Where(m => m.IsRead).ToListAsync();
            _context.ContactMessages.RemoveRange(read);
            await _context.SaveChangesAsync();
            TempData["AdminSuccess"] = read.Count == 0 ? "There are no read messages to delete." : "Read messages deleted.";
            return RedirectToAction(nameof(AdminContact));
        }


        private async Task LoadLookups(ViewmodelProduct vm)
        {
            vm.Categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            vm.Brands = await _context.Brands.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
        }

        private async Task<(string? Url, string? Error)> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return (null, null);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                return (null, "Only JPG, PNG, JFIF,  WEBP or GIF images are allowed.");
            }

            if (file.Length > MaxImageBytes)
            {
                return (null, "The image must be 5 MB or smaller.");
            }

            var folder = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + extension;
            await using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(stream);

            return ("/images/" + fileName, null);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
