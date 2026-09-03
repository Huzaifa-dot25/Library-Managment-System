using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using LibraryMS.Domain.Entities;
using LibraryMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Services;

public class MemberService : IMemberService
{
    private readonly LibraryDbContext _db;
    public MemberService(LibraryDbContext db) => _db = db;

    // ══ Sessions ══════════════════════════════════════════════════

    public async Task<List<SessionViewModel>> GetSessionsAsync() =>
        await _db.Sessions.OrderByDescending(s => s.StartDate)
            .Select(s => new SessionViewModel
            {
                SessionId   = s.SessionId,
                SessionName = s.SessionName,
                StartDate   = s.StartDate,
                EndDate     = s.EndDate
            }).ToListAsync();

    public async Task<SessionViewModel> SaveSessionAsync(SessionViewModel vm)
    {
        if (vm.SessionId == 0)
        {
            var e = new Session { SessionName = vm.SessionName.Trim(), StartDate = vm.StartDate, EndDate = vm.EndDate };
            _db.Sessions.Add(e);
            await _db.SaveChangesAsync();
            vm.SessionId = e.SessionId;
        }
        else
        {
            var e = await _db.Sessions.FindAsync(vm.SessionId) ?? throw new KeyNotFoundException();
            e.SessionName = vm.SessionName.Trim();
            e.StartDate   = vm.StartDate;
            e.EndDate     = vm.EndDate;
            await _db.SaveChangesAsync();
        }
        return vm;
    }

    public async Task<bool> DeleteSessionAsync(int id)
    {
        var e = await _db.Sessions.FindAsync(id);
        if (e == null) return false;
        _db.Sessions.Remove(e);
        await _db.SaveChangesAsync();
        return true;
    }

    // ══ Class Levels ══════════════════════════════════════════════

    public async Task<List<ClassLevelViewModel>> GetClassLevelsAsync() =>
        await _db.ClassLevels.OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new ClassLevelViewModel
            {
                ClassLevelId = c.ClassLevelId,
                Name         = c.Name,
                SortOrder    = c.SortOrder
            }).ToListAsync();

    public async Task<ClassLevelViewModel> SaveClassLevelAsync(ClassLevelViewModel vm)
    {
        if (vm.ClassLevelId == 0)
        {
            var e = new ClassLevel { Name = vm.Name.Trim(), SortOrder = vm.SortOrder };
            _db.ClassLevels.Add(e);
            await _db.SaveChangesAsync();
            vm.ClassLevelId = e.ClassLevelId;
        }
        else
        {
            var e = await _db.ClassLevels.FindAsync(vm.ClassLevelId) ?? throw new KeyNotFoundException();
            e.Name      = vm.Name.Trim();
            e.SortOrder = vm.SortOrder;
            await _db.SaveChangesAsync();
        }
        return vm;
    }

    public async Task<bool> DeleteClassLevelAsync(int id)
    {
        var e = await _db.ClassLevels.FindAsync(id);
        if (e == null) return false;
        _db.ClassLevels.Remove(e);
        await _db.SaveChangesAsync();
        return true;
    }

    // ══ Sections ══════════════════════════════════════════════════

    public async Task<List<SectionViewModel>> GetSectionsAsync(int? classLevelId = null)
    {
        var q = _db.Sections.Include(s => s.ClassLevel).AsQueryable();
        if (classLevelId.HasValue) q = q.Where(s => s.ClassLevelId == classLevelId);
        return await q.OrderBy(s => s.Name)
            .Select(s => new SectionViewModel
            {
                SectionId      = s.SectionId,
                ClassLevelId   = s.ClassLevelId,
                ClassLevelName = s.ClassLevel.Name,
                Name           = s.Name
            }).ToListAsync();
    }

    public async Task<SectionViewModel> SaveSectionAsync(SectionViewModel vm)
    {
        if (vm.SectionId == 0)
        {
            var e = new Section { ClassLevelId = vm.ClassLevelId, Name = vm.Name.Trim() };
            _db.Sections.Add(e);
            await _db.SaveChangesAsync();
            vm.SectionId = e.SectionId;
        }
        else
        {
            var e = await _db.Sections.FindAsync(vm.SectionId) ?? throw new KeyNotFoundException();
            e.ClassLevelId = vm.ClassLevelId;
            e.Name         = vm.Name.Trim();
            await _db.SaveChangesAsync();
        }
        return vm;
    }

    public async Task<bool> DeleteSectionAsync(int id)
    {
        var e = await _db.Sections.FindAsync(id);
        if (e == null) return false;
        _db.Sections.Remove(e);
        await _db.SaveChangesAsync();
        return true;
    }

    // ══ Students ══════════════════════════════════════════════════

    public async Task<List<StudentViewModel>> GetStudentsAsync(MemberFilterViewModel f)
    {
        var q = _db.Students
            .Include(s => s.Session)
            .Include(s => s.ClassLevel)
            .Include(s => s.Section)
            .AsQueryable();

        if (f.SessionId.HasValue)    q = q.Where(s => s.SessionId    == f.SessionId);
        if (f.ClassLevelId.HasValue) q = q.Where(s => s.ClassLevelId == f.ClassLevelId);
        if (f.SectionId.HasValue)    q = q.Where(s => s.SectionId    == f.SectionId);

        q = f.ActiveStatus switch
        {
            "Active"   => q.Where(s => s.IsActive),
            "Inactive" => q.Where(s => !s.IsActive),
            _          => q
        };

        if (!string.IsNullOrWhiteSpace(f.SearchTerm))
        {
            var t = f.SearchTerm.Trim().ToLower();
            q = f.SearchBy switch
            {
                "RegistrationNo" => q.Where(s => s.RegistrationNo != null && s.RegistrationNo.ToLower().Contains(t)),
                "RollNo"         => q.Where(s => s.RollNo != null && s.RollNo.ToLower().Contains(t)),
                _                => q.Where(s => s.Name.ToLower().Contains(t))
            };
        }

        return await q.OrderBy(s => s.Name)
            .Select(s => new StudentViewModel
            {
                StudentId      = s.StudentId,
                RegistrationNo = s.RegistrationNo,
                RollNo         = s.RollNo,
                Name           = s.Name,
                SessionId      = s.SessionId,
                SessionName    = s.Session != null ? s.Session.SessionName : null,
                ClassLevelId   = s.ClassLevelId,
                ClassLevelName = s.ClassLevel != null ? s.ClassLevel.Name : null,
                SectionId      = s.SectionId,
                SectionName    = s.Section != null ? s.Section.Name : null,
                IsActive       = s.IsActive
            }).ToListAsync();
    }

    public async Task<StudentViewModel> GetStudentFormAsync(int studentId = 0)
    {
        var vm = new StudentViewModel
        {
            Sessions    = await GetSessionsAsync(),
            ClassLevels = await GetClassLevelsAsync(),
            Sections    = await GetSectionsAsync()
        };

        if (studentId > 0)
        {
            var s = await _db.Students.FindAsync(studentId);
            if (s != null)
            {
                vm.StudentId      = s.StudentId;
                vm.RegistrationNo = s.RegistrationNo;
                vm.RollNo         = s.RollNo;
                vm.Name           = s.Name;
                vm.SessionId      = s.SessionId;
                vm.ClassLevelId   = s.ClassLevelId;
                vm.SectionId      = s.SectionId;
                vm.IsActive       = s.IsActive;
            }
        }
        return vm;
    }

    public async Task<StudentViewModel> SaveStudentAsync(StudentViewModel vm)
    {
        if (vm.StudentId == 0)
        {
            var e = new Student
            {
                RegistrationNo = vm.RegistrationNo?.Trim(),
                RollNo         = vm.RollNo?.Trim(),
                Name           = vm.Name.Trim(),
                SessionId      = vm.SessionId,
                ClassLevelId   = vm.ClassLevelId,
                SectionId      = vm.SectionId,
                IsActive       = vm.IsActive
            };
            _db.Students.Add(e);
            await _db.SaveChangesAsync();
            vm.StudentId = e.StudentId;
        }
        else
        {
            var e = await _db.Students.FindAsync(vm.StudentId) ?? throw new KeyNotFoundException();
            e.RegistrationNo = vm.RegistrationNo?.Trim();
            e.RollNo         = vm.RollNo?.Trim();
            e.Name           = vm.Name.Trim();
            e.SessionId      = vm.SessionId;
            e.ClassLevelId   = vm.ClassLevelId;
            e.SectionId      = vm.SectionId;
            e.IsActive       = vm.IsActive;
            await _db.SaveChangesAsync();
        }
        return vm;
    }

    public async Task<bool> DeleteStudentAsync(int id)
    {
        var e = await _db.Students.FindAsync(id);
        if (e == null) return false;
        _db.Students.Remove(e);
        await _db.SaveChangesAsync();
        return true;
    }

    // ══ Employees ═════════════════════════════════════════════════

    public async Task<List<EmployeeViewModel>> GetEmployeesAsync(string? searchTerm, string activeStatus)
    {
        var q = _db.Employees.AsQueryable();

        q = activeStatus switch
        {
            "Active"   => q.Where(e => e.IsActive),
            "Inactive" => q.Where(e => !e.IsActive),
            _          => q
        };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var t = searchTerm.Trim().ToLower();
            q = q.Where(e => e.Name.ToLower().Contains(t)
                           || (e.EmployeeCode != null && e.EmployeeCode.ToLower().Contains(t))
                           || (e.Department   != null && e.Department.ToLower().Contains(t)));
        }

        return await q.OrderBy(e => e.Name)
            .Select(e => new EmployeeViewModel
            {
                EmployeeId   = e.EmployeeId,
                Name         = e.Name,
                EmployeeCode = e.EmployeeCode,
                Department   = e.Department,
                IsActive     = e.IsActive
            }).ToListAsync();
    }

    public async Task<EmployeeViewModel?> GetEmployeeByIdAsync(int id)
    {
        var e = await _db.Employees.FindAsync(id);
        if (e == null) return null;
        return new EmployeeViewModel
        {
            EmployeeId   = e.EmployeeId,
            Name         = e.Name,
            EmployeeCode = e.EmployeeCode,
            Department   = e.Department,
            IsActive     = e.IsActive
        };
    }

    public async Task<EmployeeViewModel> SaveEmployeeAsync(EmployeeViewModel vm)
    {
        if (vm.EmployeeId == 0)
        {
            var e = new Employee
            {
                Name         = vm.Name.Trim(),
                EmployeeCode = vm.EmployeeCode?.Trim(),
                Department   = vm.Department?.Trim(),
                IsActive     = vm.IsActive
            };
            _db.Employees.Add(e);
            await _db.SaveChangesAsync();
            vm.EmployeeId = e.EmployeeId;
        }
        else
        {
            var e = await _db.Employees.FindAsync(vm.EmployeeId) ?? throw new KeyNotFoundException();
            e.Name         = vm.Name.Trim();
            e.EmployeeCode = vm.EmployeeCode?.Trim();
            e.Department   = vm.Department?.Trim();
            e.IsActive     = vm.IsActive;
            await _db.SaveChangesAsync();
        }
        return vm;
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var e = await _db.Employees.FindAsync(id);
        if (e == null) return false;
        _db.Employees.Remove(e);
        await _db.SaveChangesAsync();
        return true;
    }

    // ══ Cross-member search (Phase 6 entry point) ═════════════════

    public async Task<List<MemberSearchResultViewModel>> GetMembersAsync(
        int? sessionId, int? classLevelId, int? sectionId,
        string activeStatus, string searchBy, string? searchTerm,
        string memberType)
    {
        var results = new List<MemberSearchResultViewModel>();

        if (memberType != "Employee")
        {
            var filter = new MemberFilterViewModel
            {
                MemberType   = "Student",
                SessionId    = sessionId,
                ClassLevelId = classLevelId,
                SectionId    = sectionId,
                ActiveStatus = activeStatus,
                SearchBy     = searchBy,
                SearchTerm   = searchTerm
            };
            var students = await GetStudentsAsync(filter);
            results.AddRange(students.Select(s => new MemberSearchResultViewModel
            {
                MemberType     = "Student",
                MemberId       = s.StudentId,
                Name           = s.Name,
                RegistrationNo = s.RegistrationNo,
                RollNo         = s.RollNo,
                ClassLevel     = s.ClassLevelName,
                Section        = s.SectionName,
                Session        = s.SessionName,
                IsActive       = s.IsActive,
                IssuedBooksCount = 0
            }));
        }

        if (memberType != "Student")
        {
            var employees = await GetEmployeesAsync(searchTerm, activeStatus);
            results.AddRange(employees.Select(e => new MemberSearchResultViewModel
            {
                MemberType   = "Employee",
                MemberId     = e.EmployeeId,
                Name         = e.Name,
                RegistrationNo = e.EmployeeCode,
                Department   = e.Department,
                IsActive     = e.IsActive,
                IssuedBooksCount = 0
            }));
        }

        return results;
    }
}
