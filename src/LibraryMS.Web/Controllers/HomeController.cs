using LibraryMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDashboardService _dash;
    public HomeController(IDashboardService dash) => _dash = dash;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";
        var stats = await _dash.GetStatsAsync();
        return View(stats);
    }
}
