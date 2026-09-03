using LibraryMS.Application.ViewModels;

namespace LibraryMS.Application.Interfaces;

public interface IPublisherService
{
    Task<List<PublisherViewModel>> GetAllAsync();
    Task<PublisherViewModel?> GetByIdAsync(int id);
    Task<PublisherViewModel> SaveAsync(PublisherViewModel vm);
    Task<bool> DeleteAsync(int id);
}
