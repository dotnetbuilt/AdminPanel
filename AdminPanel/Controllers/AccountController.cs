using AdminPanel.Data;
using AdminPanel.Enums;
using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _appDbContext;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, AppDbContext appDbContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appDbContext = appDbContext;
        }

        public async Task<IActionResult> GetAll()
        { 
            var users = await _appDbContext.Users.ToListAsync();

            List<ViewModel> models = [];

            foreach(var user in users)
            {
                var model = new ViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Status = user.Status,
                    LastLoginTime = user.LastLoginTime,
                    RegistrationTime = user.RegistrationTime
                };

                models.Add(model);
            }

            return View(models); 
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if(ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.RememberMe, false);

                if (result.Succeeded)
                {
                    var user =await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                    user!.LastLoginTime = DateTime.Now;
                    await _appDbContext.SaveChangesAsync();

                    return RedirectToAction("GetAll");
                }

                ModelState.AddModelError("", "Invalid login attempt!");
                return View(model);
            }

            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                User user = new()
                {
                    Name = model.Name,
                    UserName = model.Email,
                    Email = model.Email,
                    RegistrationTime = DateTime.Now,
                    Status = Status.Active
                };

                var result = await _userManager.CreateAsync(user, model.Password!);

                if(result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("GetAll");
                }

                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("GetAll");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Block(List<ViewModel> models)
        {
            var selectedIds = models.Where(m => m.IsChecked == true).Select(m => m.Id);

            var users = await _appDbContext.Users.ToListAsync();

            foreach (var user in users)
            {
                if (selectedIds.Contains(user.Id))
                {
                    user.Status = Status.Blocked;
                    await _appDbContext.SaveChangesAsync();
                }
            }

            return RedirectToAction("GetAll");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Unblock(List<ViewModel> models)
        {
            var selectedIds = models.Where(m => m.IsChecked == true).Select(m => m.Id);

            var users = await _appDbContext.Users.ToListAsync();

            foreach (var user in users)
            {
                if (selectedIds.Contains(user.Id))
                {
                    user.Status = Status.Active;
                    await _appDbContext.SaveChangesAsync();
                }
            }

            return RedirectToAction("GetAll");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(List<ViewModel> models)
        {
            var selectedIds = models.Where(m => m.IsChecked == true).Select(m => m.Id);

            var users = await _appDbContext.Users.ToListAsync();

            foreach (var user in users)
            {
                if (selectedIds.Contains(user.Id))
                {
                    await _userManager.DeleteAsync(user);
                }
            }

            return RedirectToAction("GetAll");
        }
    }
}
