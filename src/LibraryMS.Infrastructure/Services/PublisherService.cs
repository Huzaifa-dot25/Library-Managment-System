using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using LibraryMS.Domain.Entities;
using LibraryMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Services;

public class PublisherService : IPublisherService
{
    private readonly LibraryDbContext _db;

    public PublisherService(LibraryDbContext db) => _db = db;

    public async Task<List<PublisherViewModel>> GetAllAsync() =>
        await _db.Publishers
            .OrderBy(p => p.Name)
            .Select(p => new PublisherViewModel
            {
                PublisherId = p.PublisherId,
                Name        = p.Name,
                Email       = p.Email,
                PhoneNo     = p.PhoneNo,
                Address     = p.Address,
                Gender      = p.Gender,
                Country     = p.Country,
                City        = p.City
            })
            .ToListAsync();

    public async Task<PublisherViewModel?> GetByIdAsync(int id)
    {
        var p = await _db.Publishers.FindAsync(id);
        if (p == null) return null;
        return new PublisherViewModel
        {
            PublisherId = p.PublisherId,
            Name        = p.Name,
            Email       = p.Email,
            PhoneNo     = p.PhoneNo,
            Address     = p.Address,
            Gender      = p.Gender,
            Country     = p.Country,
            City        = p.City
        };
    }

    public async Task<PublisherViewModel> SaveAsync(PublisherViewModel vm)
    {
        if (vm.PublisherId == 0)
        {
            var entity = new Publisher
            {
                Name    = vm.Name.Trim(),
                Email   = vm.Email?.Trim(),
                PhoneNo = vm.PhoneNo?.Trim(),
                Address = vm.Address?.Trim(),
                Gender  = vm.Gender?.Trim(),
                Country = vm.Country?.Trim(),
                City    = vm.City?.Trim()
            };
            _db.Publishers.Add(entity);
            await _db.SaveChangesAsync();
            vm.PublisherId = entity.PublisherId;
        }
        else
        {
            var entity = await _db.Publishers.FindAsync(vm.PublisherId)
                ?? throw new KeyNotFoundException($"Publisher {vm.PublisherId} not found.");
            entity.Name    = vm.Name.Trim();
            entity.Email   = vm.Email?.Trim();
            entity.PhoneNo = vm.PhoneNo?.Trim();
            entity.Address = vm.Address?.Trim();
            entity.Gender  = vm.Gender?.Trim();
            entity.Country = vm.Country?.Trim();
            entity.City    = vm.City?.Trim();
            await _db.SaveChangesAsync();
        }
        return vm;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Publishers.FindAsync(id);
        if (entity == null) return false;
        bool inUse = await _db.Books.AnyAsync(b => b.PublisherId == id);
        if (inUse)
            throw new InvalidOperationException(
                "Cannot delete this publisher because they are assigned to one or more books. " +
                "Update those books first.");
        _db.Publishers.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}
