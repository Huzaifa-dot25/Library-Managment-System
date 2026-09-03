using LibraryMS.Domain.Entities;
using LibraryMS.Domain.Enums;
 using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Data;

/// <summary>
/// Inherits IdentityDbContext so ASP.NET Core Identity tables
/// (AspNetUsers, AspNetRoles, etc.) are created in the same database.
/// </summary>
public class LibraryDbContext : IdentityDbContext<IdentityUser>
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    // ── DbSets ──────────────────────────────────────────────────────────────
    public DbSet<Author>       Authors       => Set<Author>();
    public DbSet<Publisher>    Publishers    => Set<Publisher>();
    public DbSet<Category>     Categories    => Set<Category>();
    public DbSet<BookType>     BookTypes     => Set<BookType>();
    public DbSet<Vendor>       Vendors       => Set<Vendor>();
    public DbSet<Book>         Books         => Set<Book>();
    public DbSet<BookAuthor>   BookAuthors   => Set<BookAuthor>();
    public DbSet<BookCategory> BookCategories => Set<BookCategory>();
    public DbSet<Session>      Sessions      => Set<Session>();
    public DbSet<ClassLevel>   ClassLevels   => Set<ClassLevel>();
    public DbSet<Section>      Sections      => Set<Section>();
    public DbSet<Student>      Students      => Set<Student>();
    public DbSet<Employee>     Employees     => Set<Employee>();
    public DbSet<Purchase>     Purchases     => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<IssuedBook>   IssuedBooks   => Set<IssuedBook>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // must be first — configures Identity tables

        // ── Author ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Author>(e =>
        {
            e.HasKey(a => a.AuthorId);
            e.Property(a => a.Name).HasMaxLength(150).IsRequired();
            e.Property(a => a.Gender).HasMaxLength(10);
            e.Property(a => a.Country).HasMaxLength(100);
            e.HasIndex(a => a.Name);
        });

        // ── Publisher ────────────────────────────────────────────────────────
        modelBuilder.Entity<Publisher>(e =>
        {
            e.HasKey(p => p.PublisherId);
            e.Property(p => p.Name).HasMaxLength(150).IsRequired();
            e.Property(p => p.Email).HasMaxLength(100);
            e.Property(p => p.PhoneNo).HasMaxLength(30);
            e.Property(p => p.Address).HasMaxLength(300);
            e.Property(p => p.Gender).HasMaxLength(10);
            e.Property(p => p.Country).HasMaxLength(100);
            e.Property(p => p.City).HasMaxLength(100);
        });

        // ── Category ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.CategoryId);
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
        });

        // ── BookType ─────────────────────────────────────────────────────────
        modelBuilder.Entity<BookType>(e =>
        {
            e.HasKey(bt => bt.BookTypeId);
            e.Property(bt => bt.TypeName).HasMaxLength(100).IsRequired();
            e.Property(bt => bt.LinkedAccount)
             .HasConversion<string>()   // stores "LibraryBooks" / "AcademicBooks"
             .HasMaxLength(30);
            e.Property(bt => bt.IsActive).HasDefaultValue(true);
        });

        // ── Vendor ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Vendor>(e =>
        {
            e.HasKey(v => v.VendorId);
            e.Property(v => v.Name).HasMaxLength(150).IsRequired();
            e.Property(v => v.ContactInfo).HasMaxLength(500);
        });

        // ── Book ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Book>(e =>
        {
            e.HasKey(b => b.BookId);
            e.Property(b => b.Title).HasMaxLength(300).IsRequired();
            e.Property(b => b.ISBN).HasMaxLength(30);
            e.Property(b => b.BarCode).HasMaxLength(50);
            e.Property(b => b.BookStatus).HasMaxLength(50);
            e.Property(b => b.RackOrGrade).HasMaxLength(100);
            e.Property(b => b.AccessionNumber).HasMaxLength(50);
            e.Property(b => b.CallNumberDeweyCode).HasMaxLength(50);
            e.Property(b => b.Price).HasPrecision(10, 2);
            e.Property(b => b.RackName).HasMaxLength(100);
            e.Property(b => b.LibrarianRemarks).HasMaxLength(500);
            e.Property(b => b.CoverImagePath).HasMaxLength(500);
            e.Property(b => b.TotalQuantity).HasDefaultValue(0);
            e.Property(b => b.RemainingQuantity).HasDefaultValue(0);
            e.Property(b => b.IssuedCount).HasDefaultValue(0);
            e.Property(b => b.LostCount).HasDefaultValue(0);
            e.Property(b => b.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            e.HasIndex(b => b.Title);
            e.HasIndex(b => b.ISBN).IsUnique().HasFilter("[ISBN] IS NOT NULL");
            e.HasIndex(b => b.BarCode);

            // Relationships
            e.HasOne(b => b.Publisher)
             .WithMany(p => p.Books)
             .HasForeignKey(b => b.PublisherId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(b => b.Vendor)
             .WithMany(v => v.Books)
             .HasForeignKey(b => b.VendorId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(b => b.BookType)
             .WithMany(bt => bt.Books)
             .HasForeignKey(b => b.BookTypeId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── BookAuthor (composite PK) ─────────────────────────────────────────
        modelBuilder.Entity<BookAuthor>(e =>
        {
            e.HasKey(ba => new { ba.BookId, ba.AuthorId });

            e.HasOne(ba => ba.Book)
             .WithMany(b => b.BookAuthors)
             .HasForeignKey(ba => ba.BookId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ba => ba.Author)
             .WithMany(a => a.BookAuthors)
             .HasForeignKey(ba => ba.AuthorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── BookCategory (composite PK) ───────────────────────────────────────
        modelBuilder.Entity<BookCategory>(e =>
        {
            e.HasKey(bc => new { bc.BookId, bc.CategoryId });

            e.HasOne(bc => bc.Book)
             .WithMany(b => b.BookCategories)
             .HasForeignKey(bc => bc.BookId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(bc => bc.Category)
             .WithMany(c => c.BookCategories)
             .HasForeignKey(bc => bc.CategoryId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Session ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Session>(e =>
        {
            e.HasKey(s => s.SessionId);
            e.Property(s => s.SessionName).HasMaxLength(50).IsRequired();
        });

        // ── ClassLevel ───────────────────────────────────────────────────────
        modelBuilder.Entity<ClassLevel>(e =>
        {
            e.HasKey(cl => cl.ClassLevelId);
            e.Property(cl => cl.Name).HasMaxLength(100).IsRequired();
            e.Property(cl => cl.SortOrder).HasDefaultValue(0);
        });

        // ── Section ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Section>(e =>
        {
            e.HasKey(s => s.SectionId);
            e.Property(s => s.Name).HasMaxLength(50).IsRequired();

            e.HasOne(s => s.ClassLevel)
             .WithMany(cl => cl.Sections)
             .HasForeignKey(s => s.ClassLevelId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Student ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Student>(e =>
        {
            e.HasKey(s => s.StudentId);
            e.Property(s => s.Name).HasMaxLength(150).IsRequired();
            e.Property(s => s.RegistrationNo).HasMaxLength(50);
            e.Property(s => s.RollNo).HasMaxLength(20);
            e.Property(s => s.IsActive).HasDefaultValue(true);

            e.HasIndex(s => s.RegistrationNo);

            e.HasOne(s => s.Session)
             .WithMany(ss => ss.Students)
             .HasForeignKey(s => s.SessionId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(s => s.ClassLevel)
             .WithMany(cl => cl.Students)
             .HasForeignKey(s => s.ClassLevelId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(s => s.Section)
             .WithMany(sec => sec.Students)
             .HasForeignKey(s => s.SectionId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Employee ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasKey(em => em.EmployeeId);
            e.Property(em => em.Name).HasMaxLength(150).IsRequired();
            e.Property(em => em.EmployeeCode).HasMaxLength(50);
            e.Property(em => em.Department).HasMaxLength(100);
            e.Property(em => em.IsActive).HasDefaultValue(true);

            e.HasIndex(em => em.EmployeeCode)
             .IsUnique()
             .HasFilter("[EmployeeCode] IS NOT NULL");
        });

        // ── Purchase ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Purchase>(e =>
        {
            e.HasKey(p => p.PurchaseId);
            e.Property(p => p.PurchaseNumber).HasMaxLength(30).IsRequired();
            e.Property(p => p.ManualPurchaseNumber).HasMaxLength(50);
            e.Property(p => p.Subject).HasMaxLength(300);
            e.Property(p => p.TotalAmount).HasPrecision(10, 2).HasDefaultValue(0);
            e.Property(p => p.OverallDiscount).HasPrecision(10, 2).HasDefaultValue(0);
            e.Property(p => p.NetAmount).HasPrecision(10, 2).HasDefaultValue(0);
            e.Property(p => p.InvoiceOrOrder).HasConversion<string>().HasMaxLength(10);

            e.HasIndex(p => p.PurchaseNumber).IsUnique();

            e.HasOne(p => p.Vendor)
             .WithMany(v => v.Purchases)
             .HasForeignKey(p => p.VendorId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── PurchaseItem ──────────────────────────────────────────────────────
        modelBuilder.Entity<PurchaseItem>(e =>
        {
            e.HasKey(pi => pi.PurchaseItemId);
            e.Property(pi => pi.ItemName).HasMaxLength(300);
            e.Property(pi => pi.SerialNo).HasMaxLength(50);
            e.Property(pi => pi.PricePerUnit).HasPrecision(10, 2);
            e.Property(pi => pi.DiscountPercent).HasPrecision(5, 2).HasDefaultValue(0);
            e.Property(pi => pi.NetPrice).HasPrecision(10, 2);

            e.HasOne(pi => pi.Purchase)
             .WithMany(p => p.PurchaseItems)
             .HasForeignKey(pi => pi.PurchaseId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(pi => pi.Book)
             .WithMany(b => b.PurchaseItems)
             .HasForeignKey(pi => pi.BookId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── IssuedBook ───────────────────────────────────────────────────────
        modelBuilder.Entity<IssuedBook>(e =>
        {
            e.HasKey(ib => ib.IssuedBookId);
            e.Property(ib => ib.IssueNo).HasMaxLength(30).IsRequired();
            e.Property(ib => ib.IssuedDetail).HasMaxLength(500);
            e.Property(ib => ib.MemberType).HasConversion<string>().HasMaxLength(10);

            e.HasIndex(ib => ib.IssueNo).IsUnique();
            e.HasIndex(ib => ib.BookId);
            e.HasIndex(ib => ib.StudentId);
            e.HasIndex(ib => ib.EmployeeId);

            e.HasOne(ib => ib.Book)
             .WithMany(b => b.IssuedBooks)
             .HasForeignKey(ib => ib.BookId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(ib => ib.Student)
             .WithMany(s => s.IssuedBooks)
             .HasForeignKey(ib => ib.StudentId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(ib => ib.Employee)
             .WithMany(em => em.IssuedBooks)
             .HasForeignKey(ib => ib.EmployeeId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
