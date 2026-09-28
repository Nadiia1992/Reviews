using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reviews.Models;

namespace Reviews.Controllers
{
    public class HomeController : Controller
    {
        private readonly MessageContext _context;

        public HomeController(MessageContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var messages = _context.Messages.Include(p => p.User);
            return View(await messages.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}