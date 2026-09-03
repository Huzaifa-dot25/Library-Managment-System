using System.ComponentModel.DataAnnotations;

namespace LibraryMS.Application.ViewModels;

public class AuthorViewModel
{
    public int AuthorId { get; set; }

    [Required(ErrorMessage = "Author name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }
}
