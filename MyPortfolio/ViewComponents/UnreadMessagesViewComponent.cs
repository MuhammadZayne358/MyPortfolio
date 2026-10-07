using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Data;

namespace MyPortfolio.ViewComponents
{
    // Shows the unread contact message count in the admin sidebar.
    public class UnreadMessagesViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public UnreadMessagesViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var unread = await _context.ContactMessages.CountAsync(m => !m.IsRead);
            return View(unread);
        }
    }
}
