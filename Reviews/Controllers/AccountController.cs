using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reviews.Models;
using System.Security.Cryptography;
using System.Text;
using static System.Net.WebRequestMethods;

namespace Reviews.Controllers
{
    public class AccountController(MessageContext context, IWebHostEnvironment appEnvironment) : Controller
    {
        private readonly MessageContext _context = context;

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel logon)
        {
            if (!ModelState.IsValid) return View(logon);

            
            var user = await _context.User.FirstOrDefaultAsync(a => a.Login == logon.Login);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Невірний логін або пароль!");
                return View(logon);
            }

            
            byte[] passwordBytes = Encoding.Unicode.GetBytes(user.Salt + logon.Password);

            
            string hash = Convert.ToHexString(SHA256.HashData(passwordBytes));

            if (user.Password != hash)
            {
                ModelState.AddModelError(string.Empty, "Невірний логін або пароль!");
                return View(logon);
            }

            HttpContext.Session.SetString("Login", user.Login!);
            HttpContext.Session.SetString("FirstName", user.FirstName!);
            HttpContext.Session.SetString("LastName", user.LastName!);

            return RedirectToAction("Clients", "Account");
        }

        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel reg)
        {
            if (!ModelState.IsValid) return View(reg);

            if (await _context.User.AnyAsync(u => u.Login == reg.Login))
            {
                ModelState.AddModelError(string.Empty, "Користувач з таким логіном вже існує!");
                return View(reg);
            }

         
            byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
            string salt = Convert.ToHexString(saltBytes);

            byte[] passwordBytes = Encoding.Unicode.GetBytes(salt + reg.Password);
            string hash = Convert.ToHexString(SHA256.HashData(passwordBytes));

            var user = new Users
            {
                FirstName = reg.FirstName,
                LastName = reg.LastName,
                Login = reg.Login,
                Salt = salt,
                Password = hash
            };

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Comment,Foto,UserId")] Message message, IFormFile? posterFile)
        {
            
            var login = HttpContext.Session.GetString("Login");

            if (string.IsNullOrEmpty(login))
            {
                return RedirectToAction("Login");
            }
     
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            message.UserId = user.Id;

            message.MessageDate = DateTime.Now;

            if (!ModelState.IsValid) return View(message);

            if (posterFile is not null && posterFile.Length > 0)
            {
                var fileName = Path.GetFileName(posterFile.FileName);
                var relativePath = $"/picture/{fileName}";
                var absolutePath = Path.Combine(appEnvironment.WebRootPath, "picture", fileName);

                await using (var fileStream = new FileStream(absolutePath, FileMode.Create))
                {
                    await posterFile.CopyToAsync(fileStream);
                }
                message.Foto = relativePath;
            }
            _context.Add(message);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Clients));
        }

        public IActionResult Client()
        {
            return View();
        }

        public async Task<IActionResult> Clients()
        {
            var messages = _context.Messages.Include(p => p.User);
            return View(await messages.ToListAsync());
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

    }


}

