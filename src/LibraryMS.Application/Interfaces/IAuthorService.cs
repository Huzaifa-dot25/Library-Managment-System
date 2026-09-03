using LibraryMS.Application.ViewModels;

namespace LibraryMS.Application.Interfaces;

public interface IAuthorService
{
    Task<List<AuthorViewModel>> GetAllAsync();
    Task<AuthorViewModel?> GetByIdAsync(int id);
    Task<AuthorViewModel> SaveAsync(AuthorViewModel vm);   // insert or update
    Task<bool> DeleteAsync(int id);
}
