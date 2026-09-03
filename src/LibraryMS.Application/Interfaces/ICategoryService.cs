using LibraryMS.Application.ViewModels;

namespace LibraryMS.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryViewModel>> GetAllAsync();
    Task<CategoryViewModel?> GetByIdAsync(int id);
    Task<CategoryViewModel> SaveAsync(CategoryViewModel vm);
    Task<bool> DeleteAsync(int id);
}
