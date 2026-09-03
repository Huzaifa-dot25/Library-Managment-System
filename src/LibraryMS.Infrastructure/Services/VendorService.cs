using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using LibraryMS.Domain.Entities;
using LibraryMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Services;

public class VendorService : IVendorService
{
    private readonly LibraryDbContext _db;
    public VendorService(LibraryDbContext db) => _db = db;

    public async Task<List<VendorViewModel>> GetAllAsync() =>
        await _db.Vendors
            .OrderBy(v => v.Name)
            .Select(v => new VendorViewModel
            {
                VendorId    = v.VendorId,
                Name        = v.Name,
                ContactInfo = v.ContactInfo
            }).ToListAsync();

    public async Task<VendorViewModel?> GetByIdAsync(int id)
    {
        var v = await _db.Vendors.FindAsync(id);
        if (v == null) return null;
        return new VendorViewModel { VendorId = v.VendorId, Name = v.Name, ContactInfo = v.ContactInfo };
    }

    public async Task<VendorViewModel> SaveAsync(VendorViewModel vm)
    {
        if (vm.VendorId == 0)
        {
            var e = new Vendor { Name = vm.Name.Trim(), ContactInfo = vm.ContactInfo?.Trim() };
            _db.Vendors.Add(e);
            await _db.SaveChangesAsync();
            vm.VendorId = e.VendorId;
        }
        else
        {
            var e = await _db.Vendors.FindAsync(vm.VendorId)
                ?? throw new KeyNotFoundException($"Vendor {vm.VendorId} not found.");
            e.Name        = vm.Name.Trim();
            e.ContactInfo = vm.ContactInfo?.Trim();
            await _db.SaveChangesAsync();
        }
        return vm;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var e = await _db.Vendors.FindAsync(id);
        if (e == null) return false;
        bool inUse = await _db.Books.AnyAsync(b => b.VendorId == id);
        if (inUse)
            throw new InvalidOperationException(
                "Cannot delete this vendor because they are assigned to one or more books. " +
                "Update those books first.");
        _db.Vendors.Remove(e);
        await _db.SaveChangesAsync();
        return true;
    }
}
