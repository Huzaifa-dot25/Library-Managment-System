using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser>   _userManager;

    public AccountController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser>   userManager)
    {
        _signInManager = signInManager;
        _userManager   = userManager;
    }

    // ── GET /Account/Login ────────────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // ── POST /Account/Login ───────────────────────────────────────
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var result = await _signInManager.PasswordSignInAsync(
            vm.Email,
            vm.Password,
            vm.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "Account locked. Too many failed attempts. Try again in 15 minutes.");
            return View(vm);
        }

        ModelState.AddModelError("", "Invalid email or password.");
        return View(vm);
    }

    // ── POST /Account/Logout ──────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    // ── GET /Account/AccessDenied ─────────────────────────────────
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
