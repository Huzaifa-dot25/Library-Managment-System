using LibraryMS.Application.ViewModels;

namespace LibraryMS.Application.Interfaces;

public interface IIssueService
{
    /// <summary>Issue a single book to a member. Checks RemainingQuantity > 0.</summary>
    Task<IssuedBookDetailViewModel> IssueBookAsync(IssueBookViewModel vm);

    /// <summary>Return one or more issued books. Increments RemainingQuantity.</summary>
    Task ReturnBooksAsync(ReturnBookViewModel vm);

    /// <summary>All books currently issued (not returned) to a member.</summary>
    Task<List<IssuedBookDetailViewModel>> GetIssuedByMemberAsync(string memberType, int memberId);

    /// <summary>Issue the same book to every member in a filtered list (bulk issue).</summary>
    Task<(int success, int skipped)> BulkIssueAsync(
        int bookId, DateTime dueDate, List<MemberSearchResultViewModel> members);

    /// <summary>All issued books (for Manage Issued Books — Phase 7).</summary>
    Task<List<IssuedBookListViewModel>> GetIssuedBooksAsync(IssuedBookFilterViewModel filter);

    /// <summary>Overdue books (for Manage Overdue Books — Phase 7).</summary>
    Task<List<IssuedBookListViewModel>> GetOverdueBooksAsync(OverdueFilterViewModel filter);
}
