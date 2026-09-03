using System.ComponentModel.DataAnnotations;

namespace LibraryMS.Application.ViewModels;

public class VendorViewModel
{
    public int VendorId { get; set; }

    [Required(ErrorMessage = "Vendor name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ContactInfo { get; set; }
}
