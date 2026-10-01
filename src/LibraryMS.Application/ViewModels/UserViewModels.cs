using System.ComponentModel.DataAnnotations;

namespace LibraryMS.Application.ViewModels;

// ── List row ──────────────────────────────────────────────────────────────
public class UserListViewModel
{
    public string  Id            { get; set; } = string.Empty;
    public string  Email         { get; set; } = string.Empty;
    public string  UserName      { get; set; } = string.Empty;
    public string  Role          { get; set; } = string.Empty;
    public bool    IsLockedOut   { get; set; }
    public bool    EmailConfirmed { get; set; }
}

// ── Create user ───────────────────────────────────────────────────────────
public class CreateUserViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the password.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a role.")]
    public string Role { get; set; } = "Librarian";
}

// ── Edit user ─────────────────────────────────────────────────────────────
public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a role.")]
    public string Role { get; set; } = "Librarian";
}

// ── Reset password ────────────────────────────────────────────────────────
public class ResetPasswordViewModel
{
    public string Id    { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the password.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm New Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
