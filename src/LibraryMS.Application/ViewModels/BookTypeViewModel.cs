using System.ComponentModel.DataAnnotations;
using LibraryMS.Domain.Enums;

namespace LibraryMS.Application.ViewModels;

public class BookTypeViewModel
{
    public int BookTypeId { get; set; }

    [Required(ErrorMessage = "Type name is required.")]
    [StringLength(100)]
    public string TypeName { get; set; } = string.Empty;

    [Required]
    public LinkedAccountType LinkedAccount { get; set; } = LinkedAccountType.LibraryBooks;

    /// <summary>Display string for the Link Account dropdown shown in the grid.</summary>
    public string LinkedAccountDisplay => LinkedAccount == LinkedAccountType.LibraryBooks
        ? "Library Books" : "Academic Books";

    public bool IsActive { get; set; } = true;
}
