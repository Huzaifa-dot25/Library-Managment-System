using LibraryMS.Application.ViewModels;

namespace LibraryMS.Application.Interfaces;

public interface IMemberService
{
    // ── Sessions ──────────────────────────────────────────────────
    Task<List<SessionViewModel>> GetSessionsAsync();
    Task<SessionViewModel> SaveSessionAsync(SessionViewModel vm);
    Task<bool> DeleteSessionAsync(int id);

    // ── Class Levels ──────────────────────────────────────────────
    Task<List<ClassLevelViewModel>> GetClassLevelsAsync();
    Task<ClassLevelViewModel> SaveClassLevelAsync(ClassLevelViewModel vm);
    Task<bool> DeleteClassLevelAsync(int id);

    // ── Sections ─────────────────────────────────────────────────
    Task<List<SectionViewModel>> GetSectionsAsync(int? classLevelId = null);
    Task<SectionViewModel> SaveSectionAsync(SectionViewModel vm);
    Task<bool> DeleteSectionAsync(int id);

    // ── Students ──────────────────────────────────────────────────
    Task<List<StudentViewModel>> GetStudentsAsync(MemberFilterViewModel filter);
    Task<StudentViewModel> GetStudentFormAsync(int studentId = 0);
    Task<StudentViewModel> SaveStudentAsync(StudentViewModel vm);
    Task<bool> DeleteStudentAsync(int id);

    // ── Employees ─────────────────────────────────────────────────
    Task<List<EmployeeViewModel>> GetEmployeesAsync(string? searchTerm, string activeStatus);
    Task<EmployeeViewModel?> GetEmployeeByIdAsync(int id);
    Task<EmployeeViewModel> SaveEmployeeAsync(EmployeeViewModel vm);
    Task<bool> DeleteEmployeeAsync(int id);

    // ── Cross-member search (used by Issue/Return, Phase 6) ───────
    Task<List<MemberSearchResultViewModel>> GetMembersAsync(
        int? sessionId, int? classLevelId, int? sectionId,
        string activeStatus, string searchBy, string? searchTerm,
        string memberType);
}
