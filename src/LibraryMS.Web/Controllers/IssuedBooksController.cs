using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

/// <summary>Stub — full implementation in Phase 7.</summary>
public class IssuedBooksController : Controller
{
    public IActionResult Index() => Content("Manage Issued Books — coming in Phase 7");

    // Stub for dashboard Issued Book Search widget
    [HttpGet]
    public IActionResult Search(string keyword) => Json(Array.Empty<object>());
}
