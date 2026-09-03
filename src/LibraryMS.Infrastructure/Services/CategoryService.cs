using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using LibraryMS.Domain.Entities;
using LibraryMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly LibraryDbContext _db;

    public CategoryService(LibraryDbContext db) => _db = db;

    public async Task<List<CategoryViewModel>> GetAllAsync() =>
        await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryViewModel
            {
                CategoryId = c.CategoryId,
                Name       = c.Name
            })
            .ToListAsync();

    public async Task<CategoryViewModel?> GetByIdAsync(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c == null) return null;
        return new CategoryViewModel { CategoryId = c.CategoryId, Name = c.Name };
    }

    public async Task<CategoryViewModel> SaveAsync(CategoryViewModel vm)
    {
        if (vm.CategoryId == 0)
        {
            var entity = new Category { Name = vm.Name.Trim() };
            _db.Categories.Add(entity);
            await _db.SaveChangesAsync();
            vm.CategoryId = entity.CategoryId;
        }
        else
        {
            var entity = await _db.Categories.FindAsync(vm.CategoryId)
                ?? throw new KeyNotFoundException($"Category {vm.CategoryId} not found.");
            entity.Name = vm.Name.Trim();
            await _db.SaveChangesAsync();
        }
        return vm;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Categories.FindAsync(id);
        if (entity == null) return false;
        bool inUse = await _db.BookCategories.AnyAsync(bc => bc.CategoryId == id);
        if (inUse)
            throw new InvalidOperationException(
                "Cannot delete this category because it is assigned to one or more books. " +
                "Remove the category from all books first.");
        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}
