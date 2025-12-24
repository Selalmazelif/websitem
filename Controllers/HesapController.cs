using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MektupProje.Data;
using MektupProje.Models.ViewModels;

namespace MektupProje.Controllers;

public class HesapController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public HesapController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Giris() => View(new LoginVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByEmailAsync(vm.Email);
        if (user is null)
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(vm);
        }

        var res = await _signInManager.PasswordSignInAsync(user, vm.Password, isPersistent: false, lockoutOnFailure: false);
        if (!res.Succeeded)
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(vm);
        }

        if (string.Equals(vm.Email, "elifselalmaz@gmail.com", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "Admin");

        return RedirectToAction("Iletisim", "Sihirbaz");
    }

    [HttpGet]
    public IActionResult Kayit() => View(new RegisterVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Kayit(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new AppUser
        {
            UserName = vm.Email,
            Email = vm.Email
        };

        var res = await _userManager.CreateAsync(user, vm.Password);

        if (!res.Succeeded)
        {
            foreach (var e in res.Errors)
                ModelState.AddModelError("", e.Description);

            return View(vm);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Iletisim", "Sihirbaz");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cikis()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Giris");
    }

    public IActionResult ErisimReddedildi() => View();
}
