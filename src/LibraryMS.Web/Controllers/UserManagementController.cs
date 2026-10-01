using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly UserManager<IdentityUser>  _userManager;
    private readonly RoleManager<IdentityRole>  _roleManager;

    public UserManagementController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ── List all users ────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "User Management";

        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var list = new List<UserListViewModel>();
        foreach (var u in users)
        {
            var roles   = await _userManager.GetRolesAsync(u);
            var lockout = await _userManager.IsLockedOutAsync(u);
            list.Add(new UserListViewModel
            {
                Id             = u.Id,
                Email          = u.Email ?? "",
                UserName       = u.UserName ?? "",
                Role           = roles.FirstOrDefault() ?? "—",
                IsLockedOut    = lockout,
                EmailConfirmed = u.EmailConfirmed
            });
        }

        return View(list);
    }

    // ── GET Create ────────────────────────────────────────────────
    public IActionResult Create()
    {
        ViewData["Title"] = "Add New User";
        return View(new CreateUserViewModel());
    }

    // ── POST Create ───────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Add New User";
            return View(vm);
        }

        // Check email not already taken
        var existing = await _userManager.FindByEmailAsync(vm.Email);
        if (existing != null)
        {
            ModelState.AddModelError("Email", "An account with this email already exists.");
            ViewData["Title"] = "Add New User";
            return View(vm);
        }

        var user = new IdentityUser
        {
            UserName       = vm.Email.Trim(),
            Email          = vm.Email.Trim(),
            EmailConfirmed = true   // confirmed by admin — no email verification needed
        };

        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);
            ViewData["Title"] = "Add New User";
            return View(vm);
        }

        // Assign role
        await _userManager.AddToRoleAsync(user, vm.Role);

        TempData["Success"] = $"User '{vm.Email}' created successfully as {vm.Role}.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET Edit ──────────────────────────────────────────────────
    public async Task<IActionResult> Edit(string id)
    {
        ViewData["Title"] = "Edit User";
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var vm = new EditUserViewModel
        {
            Id       = user.Id,
            FullName = user.UserName ?? "",
            Email    = user.Email    ?? "",
            Role     = roles.FirstOrDefault() ?? "Librarian"
        };
        return View(vm);
    }

    // ── POST Edit ─────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit User";
            return View(vm);
        }

        var user = await _userManager.FindByIdAsync(vm.Id);
        if (user == null) return NotFound();

        // Prevent editing the currently logged-in admin's own role/email
        var currentUserId = _userManager.GetUserId(User);
        if (user.Id == currentUserId && vm.Role != "Admin")
        {
            ModelState.AddModelError("Role", "You cannot remove the Admin role from your own account.");
            ViewData["Title"] = "Edit User";
            return View(vm);
        }

        // Update email / username
        user.Email    = vm.Email.Trim();
        user.UserName = vm.Email.Trim();
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var err in updateResult.Errors)
                ModelState.AddModelError("", err.Description);
            ViewData["Title"] = "Edit User";
            return View(vm);
        }

        // Update role: remove all current roles, assign new one
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, vm.Role);

        TempData["Success"] = $"User '{vm.Email}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET Reset Password ────────────────────────────────────────
    public async Task<IActionResult> ResetPassword(string id)
    {
        ViewData["Title"] = "Reset Password";
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        return View(new ResetPasswordViewModel
        {
            Id    = user.Id,
            Email = user.Email ?? ""
        });
    }

    // ── POST Reset Password ───────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Reset Password";
            return View(vm);
        }

        var user = await _userManager.FindByIdAsync(vm.Id);
        if (user == null) return NotFound();

        // Remove current password and set new one
        var token  = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);
            ViewData["Title"] = "Reset Password";
            return View(vm);
        }

        TempData["Success"] = $"Password for '{user.Email}' has been reset successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST Toggle Lockout ───────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLockout(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // Prevent locking out yourself
        var currentUserId = _userManager.GetUserId(User);
        if (user.Id == currentUserId)
        {
            TempData["Error"] = "You cannot lock out your own account.";
            return RedirectToAction(nameof(Index));
        }

        var isLocked = await _userManager.IsLockedOutAsync(user);
        if (isLocked)
        {
            // Unlock: set lockout end to now
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddSeconds(-1));
            TempData["Success"] = $"User '{user.Email}' has been unlocked.";
        }
        else
        {
            // Lock: set lockout end to far future
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            TempData["Success"] = $"User '{user.Email}' has been locked out.";
        }

        return RedirectToAction(nameof(Index));
    }

    // ── POST Delete ───────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // Prevent deleting yourself
        var currentUserId = _userManager.GetUserId(User);
        if (user.Id == currentUserId)
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.DeleteAsync(user);
        TempData["Success"] = $"User '{user.Email}' has been deleted.";
        return RedirectToAction(nameof(Index));
    }
}
