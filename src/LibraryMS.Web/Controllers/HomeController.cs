using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Home";
        return View();
    }
}
