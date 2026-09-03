using System.ComponentModel.DataAnnotations;

namespace LibraryMS.Application.ViewModels;

public class PublisherViewModel
{
    public int PublisherId { get; set; }

    [Required(ErrorMessage = "Publisher name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? PhoneNo { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(100)]
    public string? City { get; set; }
}
