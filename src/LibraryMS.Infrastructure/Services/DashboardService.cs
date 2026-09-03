using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using LibraryMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly LibraryDbContext _db;
    public DashboardService(LibraryDbContext db) => _db = db;

    public async Task<DashboardStatsViewModel> GetStatsAsync()
    {
        var today = DateTime.Today;

        return new DashboardStatsViewModel
        {
            TotalBooks      = await _db.Books.CountAsync(),
            TotalStudents   = await _db.Students.CountAsync(s => s.IsActive),
            TotalEmployees  = await _db.Employees.CountAsync(e => e.IsActive),
            BooksIssued     = await _db.IssuedBooks.CountAsync(i => i.ReturnDate == null),
            OverdueBooks    = await _db.IssuedBooks.CountAsync(i => i.ReturnDate == null && i.DueDate < today),
            TotalPurchases  = await _db.Purchases.CountAsync(),
            LowStockBooks   = await _db.Books.CountAsync(b => b.RemainingQuantity == 0 && b.TotalQuantity > 0),
            TotalCategories = await _db.Categories.CountAsync()
        };
    }
}
