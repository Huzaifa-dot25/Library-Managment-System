using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using LibraryMS.Domain.Entities;
using LibraryMS.Domain.Enums;
using LibraryMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Services;

public class BookTypeService : IBookTypeService
{
    private readonly LibraryDbContext _db;

    public BookTypeService(LibraryDbContext db) => _db = db;

    public async Task<List<BookTypeViewModel>> GetAllAsync() =>
        await _db.BookTypes
            .OrderBy(bt => bt.TypeName)
            .Select(bt => new BookTypeViewModel
            {
                BookTypeId     = bt.BookTypeId,
                TypeName       = bt.TypeName,
                LinkedAccount  = bt.LinkedAccount,
                IsActive       = bt.IsActive
            })
            .ToListAsync();

    public async Task<BookTypeViewModel?> GetByIdAsync(int id)
    {
        var bt = await _db.BookTypes.FindAsync(id);
        if (bt == null) return null;
        return new BookTypeViewModel
        {
            BookTypeId    = bt.BookTypeId,
            TypeName      = bt.TypeName,
            LinkedAccount = bt.LinkedAccount,
            IsActive      = bt.IsActive
        };
    }

    public async Task<BookTypeViewModel> SaveAsync(BookTypeViewModel vm)
    {
        if (vm.BookTypeId == 0)
        {
            var entity = new BookType
            {
                TypeName      = vm.TypeName.Trim(),
                LinkedAccount = vm.LinkedAccount,
                IsActive      = vm.IsActive
            };
            _db.BookTypes.Add(entity);
            await _db.SaveChangesAsync();
            vm.BookTypeId = entity.BookTypeId;
        }
        else
        {
            var entity = await _db.BookTypes.FindAsync(vm.BookTypeId)
                ?? throw new KeyNotFoundException($"BookType {vm.BookTypeId} not found.");
            entity.TypeName      = vm.TypeName.Trim();
            entity.LinkedAccount = vm.LinkedAccount;
            entity.IsActive      = vm.IsActive;
            await _db.SaveChangesAsync();
        }
        return vm;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.BookTypes.FindAsync(id);
        if (entity == null) return false;
        _db.BookTypes.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}
