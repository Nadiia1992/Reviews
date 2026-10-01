using Microsoft.AspNetCore.Mvc;
using Reviews.Models;
using Reviews.Repository;
using Reviews.Services;
using System.Security.Cryptography;



namespace Reviews.Controllers
{
    public class AccountController(IRepositoryMessage repo, IRepositoryUser re, PasswordHasher passwordHasher, IWebHostEnvironment appEnvironment) : Controller
    {
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel logon)
        {
            if (!ModelState.IsValid) return View(logon);


            var user = await re.GetUserByLoginAsync(logon.Login);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Невірний логін або пароль!");
                return View(logon);
            }

            if(!passwordHasher.VerifyPassword(logon.Password, user.Salt, user.Password))

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

            if (await re.UserExistsAsync(reg.Login))
            {
                ModelState.AddModelError(string.Empty, "Користувач з таким логіном вже існує!");
                return View(reg);
            }


            byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
            string salt = Convert.ToHexString(saltBytes);

            string hash = passwordHasher.HashPassword(reg.Password,salt);


            var user = new Users
            {
                FirstName = reg.FirstName,
                LastName = reg.LastName,
                Login = reg.Login,
                Salt = salt,
                Password = hash
            };

            await re.CreateAsync(user);
            await re.SaveAsync();

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

            var user = await re.GetUserByLoginAsync(login);

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
            await repo.CreateAsync(message);
            await repo.SaveAsync();
            return RedirectToAction(nameof(Clients));
        }

        public IActionResult Client()
        {
            return View();
        }

        public async Task<IActionResult> Clients(int page = 1)
        {
            var messages = await repo.GetMessageListAsync();
            return View(messages);
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}

