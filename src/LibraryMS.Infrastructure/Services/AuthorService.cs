using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using LibraryMS.Domain.Entities;
using LibraryMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Services;

public class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _db;

    public AuthorService(LibraryDbContext db) => _db = db;

    public async Task<List<AuthorViewModel>> GetAllAsync() =>
        await _db.Authors
            .OrderBy(a => a.Name)
            .Select(a => new AuthorViewModel
            {
                AuthorId = a.AuthorId,
                Name     = a.Name,
                Gender   = a.Gender,
                Country  = a.Country
            })
            .ToListAsync();

    public async Task<AuthorViewModel?> GetByIdAsync(int id)
    {
        var a = await _db.Authors.FindAsync(id);
        if (a == null) return null;
        return new AuthorViewModel
        {
            AuthorId = a.AuthorId,
            Name     = a.Name,
            Gender   = a.Gender,
            Country  = a.Country
        };
    }

    public async Task<AuthorViewModel> SaveAsync(AuthorViewModel vm)
    {
        if (vm.AuthorId == 0)
        {
            // Insert
            var entity = new Author
            {
                Name    = vm.Name.Trim(),
                Gender  = vm.Gender?.Trim(),
                Country = vm.Country?.Trim()
            };
            _db.Authors.Add(entity);
            await _db.SaveChangesAsync();
            vm.AuthorId = entity.AuthorId;
        }
        else
        {
            // Update
            var entity = await _db.Authors.FindAsync(vm.AuthorId)
                ?? throw new KeyNotFoundException($"Author {vm.AuthorId} not found.");
            entity.Name    = vm.Name.Trim();
            entity.Gender  = vm.Gender?.Trim();
            entity.Country = vm.Country?.Trim();
            await _db.SaveChangesAsync();
        }
        return vm;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Authors.FindAsync(id);
        if (entity == null) return false;
        // Check if any books reference this author
        bool inUse = await _db.BookAuthors.AnyAsync(ba => ba.AuthorId == id);
        if (inUse)
            throw new InvalidOperationException(
                "Cannot delete this author because they are assigned to one or more books. " +
                "Remove the author from all books first.");
        _db.Authors.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}
