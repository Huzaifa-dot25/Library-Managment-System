using System.ComponentModel.DataAnnotations;

namespace LibraryMS.Application.ViewModels;

public class CategoryViewModel
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
