using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

[Authorize]
public class StudentsController : Controller
{
    private readonly IMemberService _svc;
    public StudentsController(IMemberService svc) => _svc = svc;

    // ── List / Search ─────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Manage Students";
        var vm = new StudentViewModel
        {
            Sessions    = await _svc.GetSessionsAsync(),
            ClassLevels = await _svc.GetClassLevelsAsync(),
            Sections    = await _svc.GetSectionsAsync()
        };
        return View(vm);
    }

    // AJAX search
    [HttpPost]
    public async Task<IActionResult> Search([FromBody] MemberFilterViewModel filter)
    {
        var results = await _svc.GetStudentsAsync(filter);
        return Json(results);
    }

    // AJAX: sections for a class level (cascade dropdown)
    [HttpGet]
    public async Task<IActionResult> GetSections(int classLevelId)
    {
        var sections = await _svc.GetSectionsAsync(classLevelId);
        return Json(sections);
    }

    // ── Add ───────────────────────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Add Student";
        var vm = await _svc.GetStudentFormAsync(0);
        return View("StudentForm", vm);
    }

    // ── Edit ──────────────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Student";
        var vm = await _svc.GetStudentFormAsync(id);
        if (vm.StudentId == 0)
            return NotFound();
        return View("StudentForm", vm);
    }

    // ── Save ──────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(StudentViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var fresh = await _svc.GetStudentFormAsync(vm.StudentId);
            vm.Sessions    = fresh.Sessions;
            vm.ClassLevels = fresh.ClassLevels;
            vm.Sections    = fresh.Sections;
            ViewData["Title"] = vm.StudentId == 0 ? "Add Student" : "Edit Student";
            return View("StudentForm", vm);
        }
        try
        {
            await _svc.SaveStudentAsync(vm);
            TempData["Success"] = vm.StudentId == 0
                ? "Student added successfully." : "Student updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var fresh = await _svc.GetStudentFormAsync(vm.StudentId);
            vm.Sessions    = fresh.Sessions;
            vm.ClassLevels = fresh.ClassLevels;
            vm.Sections    = fresh.Sections;
            ViewData["Title"] = vm.StudentId == 0 ? "Add Student" : "Edit Student";
            return View("StudentForm", vm);
        }
    }

    // ── Delete (AJAX) ─────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] int id)
    {
        try
        {
            var ok = await _svc.DeleteStudentAsync(id);
            return Json(new { success = ok });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
