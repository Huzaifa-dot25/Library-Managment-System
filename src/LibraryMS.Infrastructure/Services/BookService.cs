using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using LibraryMS.Domain.Entities;
using LibraryMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Services;

public class BookService : IBookService
{
    private readonly LibraryDbContext _db;
    public BookService(LibraryDbContext db) => _db = db;

    // ── Paged list for Manage Books ──────────────────────────────────
    public async Task<BookPagedResult> GetPagedAsync(BookFilterViewModel f)
    {
        var q = _db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .Include(b => b.Publisher)
            .Include(b => b.BookType)
            .AsQueryable();

        // Filters
        if (f.BookTypeId.HasValue)
            q = q.Where(b => b.BookTypeId == f.BookTypeId);

        if (f.CategoryId.HasValue)
            q = q.Where(b => b.BookCategories.Any(bc => bc.CategoryId == f.CategoryId));

        if (f.AuthorId.HasValue)
            q = q.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == f.AuthorId));

        if (f.PublisherId.HasValue)
            q = q.Where(b => b.PublisherId == f.PublisherId);

        if (!string.IsNullOrWhiteSpace(f.SearchText))
        {
            var t = f.SearchText.Trim().ToLower();
            q = q.Where(b => b.Title.ToLower().Contains(t)
                           || (b.ISBN != null && b.ISBN.ToLower().Contains(t))
                           || (b.BarCode != null && b.BarCode.ToLower().Contains(t)));
        }

        if (!string.IsNullOrWhiteSpace(f.AccCallText))
        {
            var t = f.AccCallText.Trim().ToLower();
            q = q.Where(b => (b.AccessionNumber != null && b.AccessionNumber.ToLower().Contains(t))
                           || (b.CallNumberDeweyCode != null && b.CallNumberDeweyCode.ToLower().Contains(t)));
        }

        if (!string.IsNullOrWhiteSpace(f.Status))
            q = q.Where(b => b.BookStatus == f.Status);

        var total = await q.CountAsync();

        var items = await q
            .OrderBy(b => b.Title)
            .Skip((f.Page - 1) * f.PageSize)
            .Take(f.PageSize)
            .Select(b => new BookListViewModel
            {
                BookId              = b.BookId,
                Title               = b.Title,
                ISBN                = b.ISBN,
                BarCode             = b.BarCode,
                Authors             = string.Join(", ", b.BookAuthors.Select(ba => ba.Author.Name)),
                Categories          = string.Join(", ", b.BookCategories.Select(bc => bc.Category.Name)),
                Publisher           = b.Publisher != null ? b.Publisher.Name : null,
                TotalQuantity       = b.TotalQuantity,
                RemainingQuantity   = b.RemainingQuantity,
                IssuedCount         = b.IssuedCount,
                LostCount           = b.LostCount,
                RackName            = b.RackName,
                AccessionNumber     = b.AccessionNumber,
                CallNumberDeweyCode = b.CallNumberDeweyCode,
                LibrarianRemarks    = b.LibrarianRemarks,
                BookType            = b.BookType != null ? b.BookType.TypeName : null,
                CoverImagePath      = b.CoverImagePath
            })
            .ToListAsync();

        return new BookPagedResult
        {
            Items      = items,
            TotalCount = total,
            Page       = f.Page,
            PageSize   = f.PageSize
        };
    }

    // ── Load form with dropdown data ─────────────────────────────────
    public async Task<BookFormViewModel> GetFormDataAsync(int bookId = 0)
    {
        var vm = new BookFormViewModel();

        // Populate dropdowns
        vm.Authors = await _db.Authors
            .OrderBy(a => a.Name)
            .Select(a => new AuthorViewModel { AuthorId = a.AuthorId, Name = a.Name })
            .ToListAsync();

        vm.Publishers = await _db.Publishers
            .OrderBy(p => p.Name)
            .Select(p => new PublisherViewModel { PublisherId = p.PublisherId, Name = p.Name })
            .ToListAsync();

        vm.Categories = await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryViewModel { CategoryId = c.CategoryId, Name = c.Name })
            .ToListAsync();

        vm.BookTypes = await _db.BookTypes
            .Where(bt => bt.IsActive)
            .OrderBy(bt => bt.TypeName)
            .Select(bt => new BookTypeViewModel { BookTypeId = bt.BookTypeId, TypeName = bt.TypeName })
            .ToListAsync();

        vm.Vendors = await _db.Vendors
            .OrderBy(v => v.Name)
            .Select(v => new VendorViewModel { VendorId = v.VendorId, Name = v.Name })
            .ToListAsync();

        // Load existing book data for edit
        if (bookId > 0)
        {
            var b = await _db.Books
                .Include(x => x.BookAuthors)
                .Include(x => x.BookCategories)
                .FirstOrDefaultAsync(x => x.BookId == bookId);

            if (b != null)
            {
                vm.BookId               = b.BookId;
                vm.Title                = b.Title;
                vm.BookStatus           = b.BookStatus;
                vm.PublisherId          = b.PublisherId;
                vm.Description          = b.Description;
                vm.ISBN                 = b.ISBN;
                vm.Pages                = b.Pages;
                vm.BarCode              = b.BarCode;
                vm.BookKind             = b.BookStatus;
                vm.RackOrGrade          = b.RackOrGrade;
                vm.BookTypeId           = b.BookTypeId;
                vm.VendorId             = b.VendorId;
                vm.AccessionNumber      = b.AccessionNumber;
                vm.CallNumberDeweyCode  = b.CallNumberDeweyCode;
                vm.Price                = b.Price;
                vm.TotalQuantity        = b.TotalQuantity;
                vm.RackName             = b.RackName;
                vm.LibrarianRemarks     = b.LibrarianRemarks;
                vm.CoverImagePath       = b.CoverImagePath;
                vm.SelectedAuthorIds    = b.BookAuthors.Select(ba => ba.AuthorId).ToList();
                vm.SelectedCategoryIds  = b.BookCategories.Select(bc => bc.CategoryId).ToList();
            }
        }

        return vm;
    }

    // ── Save (insert or update) ──────────────────────────────────────
    public async Task<int> SaveAsync(BookFormViewModel vm, string webRootPath, Stream? imageStream = null, string? imageFileName = null)
    {
        Book book;

        if (vm.BookId == 0)
        {
            book = new Book { CreatedDate = DateTime.UtcNow };
            _db.Books.Add(book);
        }
        else
        {
            book = await _db.Books
                .Include(b => b.BookAuthors)
                .Include(b => b.BookCategories)
                .FirstOrDefaultAsync(b => b.BookId == vm.BookId)
                ?? throw new KeyNotFoundException($"Book {vm.BookId} not found.");

            // Remove existing join rows — will re-add below
            _db.BookAuthors.RemoveRange(book.BookAuthors);
            _db.BookCategories.RemoveRange(book.BookCategories);
        }

        // Map fields
        book.Title               = vm.Title.Trim();
        book.BookStatus          = vm.BookStatus?.Trim();
        book.PublisherId         = vm.PublisherId;
        book.Description         = vm.Description?.Trim();
        book.ISBN                = vm.ISBN?.Trim();
        book.Pages               = vm.Pages;
        book.BarCode             = vm.BarCode?.Trim();
        book.RackOrGrade         = vm.RackOrGrade?.Trim();
        book.BookTypeId          = vm.BookTypeId;
        book.VendorId            = vm.VendorId;
        book.AccessionNumber     = vm.AccessionNumber?.Trim();
        book.CallNumberDeweyCode = vm.CallNumberDeweyCode?.Trim();
        book.Price               = vm.Price;
        book.TotalQuantity       = vm.TotalQuantity;
        // Only set RemainingQuantity on new books.
        // On edits preserve the current remaining quantity and just adjust for
        // the delta in TotalQuantity (e.g. librarian increases stock).
        if (vm.BookId == 0)
        {
            book.RemainingQuantity = vm.TotalQuantity;
        }
        else
        {
            // Delta: if TotalQuantity was increased by N, add N to remaining.
            int delta = vm.TotalQuantity - book.TotalQuantity;
            book.RemainingQuantity = Math.Max(0, book.RemainingQuantity + delta);
        }
        book.RackName            = vm.RackName?.Trim();
        book.LibrarianRemarks    = vm.LibrarianRemarks?.Trim();

        // Cover image upload
        if (imageStream != null && imageFileName != null)
        {
            var ext = Path.GetExtension(imageFileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            if (!allowed.Contains(ext))
                throw new InvalidOperationException("Only jpg, jpeg, png, gif images are allowed.");

            var uploadsDir = Path.Combine(webRootPath, "uploads", "books");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using var fs = new FileStream(filePath, FileMode.Create);
            await imageStream.CopyToAsync(fs);
            book.CoverImagePath = $"/uploads/books/{fileName}";
        }

        await _db.SaveChangesAsync();

        // Re-add authors
        foreach (var aid in vm.SelectedAuthorIds.Distinct())
            _db.BookAuthors.Add(new BookAuthor { BookId = book.BookId, AuthorId = aid });

        // Re-add categories
        foreach (var cid in vm.SelectedCategoryIds.Distinct())
            _db.BookCategories.Add(new BookCategory { BookId = book.BookId, CategoryId = cid });

        await _db.SaveChangesAsync();
        return book.BookId;
    }

    // ── Delete ───────────────────────────────────────────────────────
    public async Task<bool> DeleteAsync(int id)
    {
        var b = await _db.Books.FindAsync(id);
        if (b == null) return false;
        bool hasActiveIssues = await _db.IssuedBooks
            .AnyAsync(i => i.BookId == id && i.ReturnDate == null);
        if (hasActiveIssues)
            throw new InvalidOperationException(
                "Cannot delete this book because it currently has copies issued out. " +
                "Return all copies first.");
        _db.Books.Remove(b);
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Book Search widget ───────────────────────────────────────────
    public async Task<List<BookSearchResultViewModel>> SearchAsync(string criteria, string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return new();

        var kw = keyword.Trim().ToLower();

        var q = _db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        q = criteria switch
        {
            "isbn"     => q.Where(b => b.ISBN != null && b.ISBN.ToLower().Contains(kw)),
            "title"    => q.Where(b => b.Title.ToLower().Contains(kw)),
            "price"    => decimal.TryParse(keyword, out var p)
                            ? q.Where(b => b.Price == p)
                            : q.Where(b => false),
            "category" => q.Where(b => b.BookCategories
                            .Any(bc => bc.Category.Name.ToLower().Contains(kw))),
            "author"   => q.Where(b => b.BookAuthors
                            .Any(ba => ba.Author.Name.ToLower().Contains(kw))),
            _          => q.Where(b => b.Title.ToLower().Contains(kw)
                            || (b.ISBN != null && b.ISBN.ToLower().Contains(kw)))
        };

        return await q
            .OrderBy(b => b.Title)
            .Take(50)
            .Select(b => new BookSearchResultViewModel
            {
                BookId            = b.BookId,
                ISBN              = b.ISBN,
                Title             = b.Title,
                Authors           = string.Join(", ", b.BookAuthors.Select(ba => ba.Author.Name)),
                TotalQuantity     = b.TotalQuantity,
                RemainingQuantity = b.RemainingQuantity
            })
            .ToListAsync();
    }
}
