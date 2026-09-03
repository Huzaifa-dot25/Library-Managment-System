using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

[Authorize]
public class OverdueBooksController : Controller
{
    private readonly IIssueService _svc;
    public OverdueBooksController(IIssueService svc) => _svc = svc;

    public IActionResult Index()
    {
        ViewData["Title"] = "Manage Overdue Books";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Search([FromBody] OverdueFilterViewModel filter)
    {
        var results = await _svc.GetOverdueBooksAsync(filter);
        return Json(results);
    }
}
