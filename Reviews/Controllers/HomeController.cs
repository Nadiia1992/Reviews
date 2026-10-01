using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reviews.Models;
using Reviews.Repository;

namespace Reviews.Controllers
{
    public class HomeController(IRepositoryMessage repo) : Controller
    {

        public async Task<IActionResult> Index()
        {
            var messages = await repo.GetMessageListAsync();
            return View(messages);
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