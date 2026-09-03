using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

/// <summary>Stub — full implementation in Phase 8.</summary>
public class ReportsController : Controller
{
    public IActionResult BooksInventory() => Content("Books Inventory Report — coming in Phase 8");
    public IActionResult IssuedBooks()    => Content("Issued Books Report — coming in Phase 8");
    public IActionResult Overdue()        => Content("Overdue Report — coming in Phase 8");
    public IActionResult Purchases()      => Content("Purchases Report — coming in Phase 8");
}
