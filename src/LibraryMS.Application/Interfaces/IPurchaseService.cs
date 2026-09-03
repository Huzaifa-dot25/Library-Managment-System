using LibraryMS.Application.ViewModels;

namespace LibraryMS.Application.Interfaces;

public interface IPurchaseService
{
    /// <summary>Returns empty form with next auto purchase number + vendor list.</summary>
    Task<PurchaseFormViewModel> GetNewFormAsync();

    /// <summary>Returns existing purchase loaded for editing.</summary>
    Task<PurchaseFormViewModel> GetForEditAsync(int purchaseId);

    /// <summary>Save (insert or update). Updates book quantities in same transaction.</summary>
    Task<int> SaveAsync(PurchaseFormViewModel vm);

    /// <summary>History search with filters.</summary>
    Task<List<PurchaseListViewModel>> SearchAsync(PurchaseFilterViewModel filter);

    /// <summary>Current stock of a book (for "Existing Stock" label in entry row).</summary>
    Task<int> GetBookStockAsync(int bookId);
}
