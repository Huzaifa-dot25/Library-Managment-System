using LibraryMS.Application.ViewModels;

namespace LibraryMS.Application.Interfaces;

public interface IVendorService
{
    Task<List<VendorViewModel>> GetAllAsync();
    Task<VendorViewModel?> GetByIdAsync(int id);
    Task<VendorViewModel> SaveAsync(VendorViewModel vm);
    Task<bool> DeleteAsync(int id);
}
