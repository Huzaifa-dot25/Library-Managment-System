using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

/// <summary>
/// Compact admin pages for Sessions, ClassLevels, and Sections.
/// All CRUD via AJAX inline grids.
/// </summary>
public class LookupController : Controller
{
    private readonly IMemberService _svc;
    public LookupController(IMemberService svc) => _svc = svc;

    // ── Sessions ──────────────────────────────────────────────────
    public IActionResult Sessions()
    {
        ViewData["Title"] = "Manage Sessions";
        return View();
    }

    [HttpGet]  public async Task<IActionResult> GetSessions()    => Json(await _svc.GetSessionsAsync());
    [HttpPost] public async Task<IActionResult> SaveSession([FromBody] SessionViewModel vm)
    {
        if (!ModelState.IsValid) return Json(new { success = false, message = "Name required." });
        try { return Json(new { success = true, data = await _svc.SaveSessionAsync(vm) }); }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }
    [HttpPost] public async Task<IActionResult> DeleteSession([FromBody] int id)
    {
        try { return Json(new { success = await _svc.DeleteSessionAsync(id) }); }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

    // ── Class Levels ──────────────────────────────────────────────
    public IActionResult ClassLevels()
    {
        ViewData["Title"] = "Manage Class Levels";
        return View();
    }

    [HttpGet]  public async Task<IActionResult> GetClassLevels()  => Json(await _svc.GetClassLevelsAsync());
    [HttpPost] public async Task<IActionResult> SaveClassLevel([FromBody] ClassLevelViewModel vm)
    {
        if (!ModelState.IsValid) return Json(new { success = false, message = "Name required." });
        try { return Json(new { success = true, data = await _svc.SaveClassLevelAsync(vm) }); }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }
    [HttpPost] public async Task<IActionResult> DeleteClassLevel([FromBody] int id)
    {
        try { return Json(new { success = await _svc.DeleteClassLevelAsync(id) }); }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

    // ── Sections ─────────────────────────────────────────────────
    public IActionResult Sections()
    {
        ViewData["Title"] = "Manage Sections";
        return View();
    }

    [HttpGet]  public async Task<IActionResult> GetSections(int? classLevelId) => Json(await _svc.GetSectionsAsync(classLevelId));
    [HttpPost] public async Task<IActionResult> SaveSection([FromBody] SectionViewModel vm)
    {
        if (!ModelState.IsValid) return Json(new { success = false, message = "Name required." });
        try { return Json(new { success = true, data = await _svc.SaveSectionAsync(vm) }); }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }
    [HttpPost] public async Task<IActionResult> DeleteSection([FromBody] int id)
    {
        try { return Json(new { success = await _svc.DeleteSectionAsync(id) }); }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

    // ── AJAX helpers used by other pages ─────────────────────────
    [HttpGet] public async Task<IActionResult> GetClassLevelsJson() => Json(await _svc.GetClassLevelsAsync());
    [HttpGet] public async Task<IActionResult> GetSectionsJson(int classLevelId) => Json(await _svc.GetSectionsAsync(classLevelId));
}
