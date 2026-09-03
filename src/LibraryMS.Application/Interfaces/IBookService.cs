using LibraryMS.Application.ViewModels;

namespace LibraryMS.Application.Interfaces;

public interface IBookService
{
    Task<BookPagedResult> GetPagedAsync(BookFilterViewModel filter);
    Task<BookFormViewModel> GetFormDataAsync(int bookId = 0);
    Task<int> SaveAsync(BookFormViewModel vm, string webRootPath, Stream? imageStream = null, string? imageFileName = null);
    Task<bool> DeleteAsync(int id);
    Task<List<BookSearchResultViewModel>> SearchAsync(string criteria, string keyword);
}
