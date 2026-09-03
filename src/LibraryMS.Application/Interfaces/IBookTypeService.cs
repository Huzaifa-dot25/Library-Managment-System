using LibraryMS.Application.ViewModels;

namespace LibraryMS.Application.Interfaces;

public interface IBookTypeService
{
    Task<List<BookTypeViewModel>> GetAllAsync();
    Task<BookTypeViewModel?> GetByIdAsync(int id);
    Task<BookTypeViewModel> SaveAsync(BookTypeViewModel vm);
    Task<bool> DeleteAsync(int id);
}
