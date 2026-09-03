using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

public class EmployeesController : Controller
{
    private readonly IMemberService _svc;
    public EmployeesController(IMemberService svc) => _svc = svc;

    // ── List / Search ─────────────────────────────────────────────
    public IActionResult Index()
    {
        ViewData["Title"] = "Manage Employees";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? searchTerm, string activeStatus = "Active")
    {
        var results = await _svc.GetEmployeesAsync(searchTerm, activeStatus);
        return Json(results);
    }

    // ── Add ───────────────────────────────────────────────────────
    public IActionResult Create()
    {
        ViewData["Title"] = "Add Employee";
        return View("EmployeeForm", new EmployeeViewModel());
    }

    // ── Edit ──────────────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Employee";
        var vm = await _svc.GetEmployeeByIdAsync(id);
        if (vm == null) return NotFound();
        return View("EmployeeForm", vm);
    }

    // ── Save ──────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(EmployeeViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = vm.EmployeeId == 0 ? "Add Employee" : "Edit Employee";
            return View("EmployeeForm", vm);
        }
        try
        {
            await _svc.SaveEmployeeAsync(vm);
            TempData["Success"] = vm.EmployeeId == 0
                ? "Employee added successfully." : "Employee updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewData["Title"] = vm.EmployeeId == 0 ? "Add Employee" : "Edit Employee";
            return View("EmployeeForm", vm);
        }
    }

    // ── Delete (AJAX) ─────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] int id)
    {
        try
        {
            var ok = await _svc.DeleteEmployeeAsync(id);
            return Json(new { success = ok });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
