using LibraryMS.Domain.Entities;
using LibraryMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryMS.Infrastructure.Data;

/// <summary>
/// Idempotent database seeder for LibraryMS.
/// Automatically runs pending migrations, ensures Identity roles and users,
/// and populates reference lookup and sample data on initial setup.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<LibraryDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Ensure migrations are applied
        await db.Database.MigrateAsync();

        // 2. Seed Identity Roles
        string[] roles = ["Admin", "Librarian"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 3. Seed Default Admin User
        var adminEmail = "admin@library.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // 4. Seed Default Librarian User
        var librarianEmail = "librarian@library.com";
        var librarianUser = await userManager.FindByEmailAsync(librarianEmail);
        if (librarianUser == null)
        {
            librarianUser = new IdentityUser
            {
                UserName = librarianEmail,
                Email = librarianEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(librarianUser, "Librarian@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(librarianUser, "Librarian");
            }
        }
        else if (!await userManager.IsInRoleAsync(librarianUser, "Librarian"))
        {
            await userManager.AddToRoleAsync(librarianUser, "Librarian");
        }

        // 5. Seed Book Types
        if (!await db.BookTypes.AnyAsync())
        {
            var bookTypes = new List<BookType>
            {
                new() { TypeName = "General Book", LinkedAccount = LinkedAccountType.LibraryBooks, IsActive = true },
                new() { TypeName = "Reference Book", LinkedAccount = LinkedAccountType.LibraryBooks, IsActive = true },
                new() { TypeName = "Textbook", LinkedAccount = LinkedAccountType.AcademicBooks, IsActive = true },
                new() { TypeName = "Periodical / Journal", LinkedAccount = LinkedAccountType.LibraryBooks, IsActive = true },
                new() { TypeName = "Digital Media / CD", LinkedAccount = LinkedAccountType.LibraryBooks, IsActive = true }
            };
            await db.BookTypes.AddRangeAsync(bookTypes);
            await db.SaveChangesAsync();
        }

        // 6. Seed Categories
        if (!await db.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "Computer Science" },
                new() { Name = "Software Engineering" },
                new() { Name = "Data Science & AI" },
                new() { Name = "Mathematics" },
                new() { Name = "Physics" },
                new() { Name = "Literature & Fiction" },
                new() { Name = "History & Biography" },
                new() { Name = "Business & Management" }
            };
            await db.Categories.AddRangeAsync(categories);
            await db.SaveChangesAsync();
        }

        // 7. Seed Publishers
        if (!await db.Publishers.AnyAsync())
        {
            var publishers = new List<Publisher>
            {
                new()
                {
                    Name = "O'Reilly Media",
                    Email = "info@oreilly.com",
                    PhoneNo = "+1-707-827-7000",
                    Address = "1005 Gravenstein Hwy N",
                    City = "Sebastopol",
                    Country = "USA"
                },
                new()
                {
                    Name = "Addison-Wesley Professional",
                    Email = "contact@informit.com",
                    PhoneNo = "+1-800-382-3419",
                    Address = "75 Arlington St #300",
                    City = "Boston",
                    Country = "USA"
                },
                new()
                {
                    Name = "Manning Publications",
                    Email = "support@manning.com",
                    PhoneNo = "+1-203-626-1510",
                    Address = "20 Baldwin Road",
                    City = "Shelton",
                    Country = "USA"
                },
                new()
                {
                    Name = "Pearson Education",
                    Email = "contact@pearson.com",
                    PhoneNo = "+1-800-477-6228",
                    Address = "80 Strand",
                    City = "London",
                    Country = "UK"
                },
                new()
                {
                    Name = "MIT Press",
                    Email = "mitpress-orders@mit.edu",
                    PhoneNo = "+1-617-253-5646",
                    Address = "One Rogers St",
                    City = "Cambridge",
                    Country = "USA"
                },
                new()
                {
                    Name = "Packt Publishing",
                    Email = "customercare@packt.com",
                    PhoneNo = "+44-121-265-6484",
                    Address = "Livery Place",
                    City = "Birmingham",
                    Country = "UK"
                }
            };
            await db.Publishers.AddRangeAsync(publishers);
            await db.SaveChangesAsync();
        }

        // 8. Seed Authors
        if (!await db.Authors.AnyAsync())
        {
            var authors = new List<Author>
            {
                new() { Name = "Robert C. Martin", Gender = "Male", Country = "USA" },
                new() { Name = "Martin Fowler", Gender = "Male", Country = "UK" },
                new() { Name = "Andrew Hunt", Gender = "Male", Country = "USA" },
                new() { Name = "David Thomas", Gender = "Male", Country = "USA" },
                new() { Name = "Eric Evans", Gender = "Male", Country = "USA" },
                new() { Name = "Donald E. Knuth", Gender = "Male", Country = "USA" },
                new() { Name = "Jon Skeet", Gender = "Male", Country = "UK" },
                new() { Name = "Steve McConnell", Gender = "Male", Country = "USA" }
            };
            await db.Authors.AddRangeAsync(authors);
            await db.SaveChangesAsync();
        }

        // 9. Seed Vendors
        if (!await db.Vendors.AnyAsync())
        {
            var vendors = new List<Vendor>
            {
                new() { Name = "Apex Academic Book Distributors", ContactInfo = "sales@apexbooks.com | +1-800-555-0199" },
                new() { Name = "National Book Foundation", ContactInfo = "orders@nbf.org | +1-800-555-0144" },
                new() { Name = "Global Educational Supplies", ContactInfo = "support@globaledubooks.com | +44-20-7946-0912" }
            };
            await db.Vendors.AddRangeAsync(vendors);
            await db.SaveChangesAsync();
        }

        // 10. Seed Academic Lookups (Sessions, ClassLevels, Sections)
        if (!await db.Sessions.AnyAsync())
        {
            var session2025 = new Session { SessionName = "2025-2026" };
            var session2026 = new Session { SessionName = "2026-2027" };
            await db.Sessions.AddRangeAsync(session2025, session2026);

            var classBSCS = new ClassLevel { Name = "BS Computer Science", SortOrder = 1 };
            var classBSSE = new ClassLevel { Name = "BS Software Engineering", SortOrder = 2 };
            var class9 = new ClassLevel { Name = "Class 9", SortOrder = 3 };
            var class10 = new ClassLevel { Name = "Class 10", SortOrder = 4 };
            await db.ClassLevels.AddRangeAsync(classBSCS, classBSSE, class9, class10);
            await db.SaveChangesAsync();

            var secBscsA = new Section { Name = "Section A", ClassLevelId = classBSCS.ClassLevelId };
            var secBscsB = new Section { Name = "Section B", ClassLevelId = classBSCS.ClassLevelId };
            var secBsseA = new Section { Name = "Section A", ClassLevelId = classBSSE.ClassLevelId };
            await db.Sections.AddRangeAsync(secBscsA, secBscsB, secBsseA);
            await db.SaveChangesAsync();

            // 11. Seed Sample Members (Students & Employees)
            if (!await db.Students.AnyAsync())
            {
                var students = new List<Student>
                {
                    new()
                    {
                        Name = "Alice Johnson",
                        RegistrationNo = "REG-2025-001",
                        RollNo = "CS-001",
                        IsActive = true,
                        SessionId = session2025.SessionId,
                        ClassLevelId = classBSCS.ClassLevelId,
                        SectionId = secBscsA.SectionId
                    },
                    new()
                    {
                        Name = "Bob Smith",
                        RegistrationNo = "REG-2025-002",
                        RollNo = "CS-002",
                        IsActive = true,
                        SessionId = session2025.SessionId,
                        ClassLevelId = classBSCS.ClassLevelId,
                        SectionId = secBscsB.SectionId
                    },
                    new()
                    {
                        Name = "Charlie Brown",
                        RegistrationNo = "REG-2025-003",
                        RollNo = "SE-001",
                        IsActive = true,
                        SessionId = session2025.SessionId,
                        ClassLevelId = classBSSE.ClassLevelId,
                        SectionId = secBsseA.SectionId
                    }
                };
                await db.Students.AddRangeAsync(students);
            }

            if (!await db.Employees.AnyAsync())
            {
                var employees = new List<Employee>
                {
                    new()
                    {
                        Name = "Dr. Alan Turing",
                        EmployeeCode = "EMP-101",
                        Department = "Computer Science",
                        IsActive = true
                    },
                    new()
                    {
                        Name = "Prof. Grace Hopper",
                        EmployeeCode = "EMP-102",
                        Department = "Software Engineering",
                        IsActive = true
                    }
                };
                await db.Employees.AddRangeAsync(employees);
            }
            await db.SaveChangesAsync();
        }

        // 12. Seed Books
        if (!await db.Books.AnyAsync(b => b.ISBN == "978-0132350884"))
        {
            var generalType = await db.BookTypes.FirstOrDefaultAsync(t => t.TypeName == "General Book");
            var refType = await db.BookTypes.FirstOrDefaultAsync(t => t.TypeName == "Reference Book");
            var textType = await db.BookTypes.FirstOrDefaultAsync(t => t.TypeName == "Textbook");

            var pubAddison = await db.Publishers.FirstOrDefaultAsync(p => p.Name.Contains("Addison-Wesley"));
            var pubPearson = await db.Publishers.FirstOrDefaultAsync(p => p.Name.Contains("Pearson"));
            var pubManning = await db.Publishers.FirstOrDefaultAsync(p => p.Name.Contains("Manning"));

            var vendorApex = await db.Vendors.FirstOrDefaultAsync(v => v.Name.Contains("Apex"));
            var vendorNbf = await db.Vendors.FirstOrDefaultAsync(v => v.Name.Contains("National Book"));
            var vendorGlobal = await db.Vendors.FirstOrDefaultAsync(v => v.Name.Contains("Global Educational"));

            var csCat = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Computer Science");
            var seCat = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Software Engineering");
            var mathCat = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Mathematics");

            var authMartin = await db.Authors.FirstOrDefaultAsync(a => a.Name.Contains("Martin") && a.Name.Contains("Robert"));
            var authFowler = await db.Authors.FirstOrDefaultAsync(a => a.Name.Contains("Fowler"));
            var authHunt = await db.Authors.FirstOrDefaultAsync(a => a.Name.Contains("Hunt"));
            var authThomas = await db.Authors.FirstOrDefaultAsync(a => a.Name.Contains("Thomas"));
            var authEvans = await db.Authors.FirstOrDefaultAsync(a => a.Name.Contains("Evans"));
            var authKnuth = await db.Authors.FirstOrDefaultAsync(a => a.Name.Contains("Knuth"));
            var authSkeet = await db.Authors.FirstOrDefaultAsync(a => a.Name.Contains("Skeet"));
            var authMcConnell = await db.Authors.FirstOrDefaultAsync(a => a.Name.Contains("McConnell"));

            var books = new List<Book>
            {
                new()
                {
                    Title = "Clean Code: A Handbook of Agile Software Craftsmanship",
                    ISBN = "978-0132350884",
                    BarCode = "BC-9780132350884",
                    AccessionNumber = "ACC-001",
                    CallNumberDeweyCode = "005.1 MAR",
                    Price = 44.99m,
                    TotalQuantity = 10,
                    RemainingQuantity = 10,
                    BookStatus = "New Book",
                    Pages = 464,
                    RackName = "Rack A-1",
                    LibrarianRemarks = "Core reading for software engineering students.",
                    Description = "Even bad code can function. But if code isn't clean, it can bring a development organization to its knees.",
                    BookTypeId = generalType?.BookTypeId,
                    PublisherId = pubPearson?.PublisherId,
                    VendorId = vendorApex?.VendorId
                },
                new()
                {
                    Title = "The Pragmatic Programmer: Your Journey to Mastery",
                    ISBN = "978-0135957059",
                    BarCode = "BC-9780135957059",
                    AccessionNumber = "ACC-002",
                    CallNumberDeweyCode = "005.1 HUN",
                    Price = 49.99m,
                    TotalQuantity = 8,
                    RemainingQuantity = 8,
                    BookStatus = "New Book",
                    Pages = 352,
                    RackName = "Rack A-2",
                    LibrarianRemarks = "20th Anniversary Edition.",
                    Description = "The classic guide to software craftsmanship, newly updated for modern systems.",
                    BookTypeId = generalType?.BookTypeId,
                    PublisherId = pubAddison?.PublisherId,
                    VendorId = vendorApex?.VendorId
                },
                new()
                {
                    Title = "Refactoring: Improving the Design of Existing Code",
                    ISBN = "978-0134757599",
                    BarCode = "BC-9780134757599",
                    AccessionNumber = "ACC-003",
                    CallNumberDeweyCode = "005.1 FOW",
                    Price = 54.99m,
                    TotalQuantity = 6,
                    RemainingQuantity = 6,
                    BookStatus = "New Book",
                    Pages = 448,
                    RackName = "Rack A-3",
                    LibrarianRemarks = "Essential guide on refactoring patterns.",
                    Description = "For more than twenty years, serious programmers have relied on Martin Fowler's Refactoring.",
                    BookTypeId = generalType?.BookTypeId,
                    PublisherId = pubAddison?.PublisherId,
                    VendorId = vendorNbf?.VendorId
                },
                new()
                {
                    Title = "Domain-Driven Design: Tackling Complexity in the Heart of Software",
                    ISBN = "978-0321125217",
                    BarCode = "BC-9780321125217",
                    AccessionNumber = "ACC-004",
                    CallNumberDeweyCode = "005.1 EVA",
                    Price = 59.99m,
                    TotalQuantity = 5,
                    RemainingQuantity = 5,
                    BookStatus = "New Book",
                    Pages = 560,
                    RackName = "Rack B-1",
                    LibrarianRemarks = "Advanced architecture reference.",
                    Description = "Describes a systematic approach to domain-driven design.",
                    BookTypeId = generalType?.BookTypeId,
                    PublisherId = pubAddison?.PublisherId,
                    VendorId = vendorApex?.VendorId
                },
                new()
                {
                    Title = "C# in Depth (4th Edition)",
                    ISBN = "978-1617294532",
                    BarCode = "BC-9781617294532",
                    AccessionNumber = "ACC-005",
                    CallNumberDeweyCode = "005.13 SKE",
                    Price = 42.50m,
                    TotalQuantity = 12,
                    RemainingQuantity = 12,
                    BookStatus = "New Book",
                    Pages = 528,
                    RackName = "Rack B-2",
                    LibrarianRemarks = "Covers C# 7 and modern language paradigms.",
                    Description = "The key to unlocking real C# power is grasping nuances such as string interpolation, pattern matching, and async.",
                    BookTypeId = textType?.BookTypeId,
                    PublisherId = pubManning?.PublisherId,
                    VendorId = vendorGlobal?.VendorId
                },
                new()
                {
                    Title = "Code Complete: A Practical Handbook of Software Construction",
                    ISBN = "978-0735619678",
                    BarCode = "BC-9780735619678",
                    AccessionNumber = "ACC-006",
                    CallNumberDeweyCode = "005.1 MCC",
                    Price = 48.00m,
                    TotalQuantity = 7,
                    RemainingQuantity = 7,
                    BookStatus = "New Book",
                    Pages = 960,
                    RackName = "Rack B-3",
                    LibrarianRemarks = "Comprehensive guide on construction practices.",
                    Description = "Widely considered one of the best practical guides to programming.",
                    BookTypeId = refType?.BookTypeId,
                    PublisherId = pubPearson?.PublisherId,
                    VendorId = vendorApex?.VendorId
                },
                new()
                {
                    Title = "The Art of Computer Programming, Volumes 1-4A",
                    ISBN = "978-0321751041",
                    BarCode = "BC-9780321751041",
                    AccessionNumber = "ACC-007",
                    CallNumberDeweyCode = "004 KNU",
                    Price = 199.99m,
                    TotalQuantity = 3,
                    RemainingQuantity = 3,
                    BookStatus = "New Book",
                    Pages = 3168,
                    RackName = "Rack R-1",
                    LibrarianRemarks = "Reference only — do not issue for home loan.",
                    Description = "The bible of fundamental computer programming algorithms.",
                    BookTypeId = refType?.BookTypeId,
                    PublisherId = pubAddison?.PublisherId,
                    VendorId = vendorNbf?.VendorId
                }
            };

            await db.Books.AddRangeAsync(books);
            await db.SaveChangesAsync();

            // Link BookAuthors and BookCategories
            var cleanCode = books[0];
            var pragmatic = books[1];
            var refactoring = books[2];
            var ddd = books[3];
            var csharp = books[4];
            var codeComplete = books[5];
            var taocp = books[6];

            var bookAuthors = new List<BookAuthor>();
            var bookCategories = new List<BookCategory>();

            // Clean Code
            if (authMartin != null) bookAuthors.Add(new BookAuthor { BookId = cleanCode.BookId, AuthorId = authMartin.AuthorId });
            if (csCat != null) bookCategories.Add(new BookCategory { BookId = cleanCode.BookId, CategoryId = csCat.CategoryId });
            if (seCat != null) bookCategories.Add(new BookCategory { BookId = cleanCode.BookId, CategoryId = seCat.CategoryId });

            // Pragmatic Programmer
            if (authHunt != null) bookAuthors.Add(new BookAuthor { BookId = pragmatic.BookId, AuthorId = authHunt.AuthorId });
            if (authThomas != null) bookAuthors.Add(new BookAuthor { BookId = pragmatic.BookId, AuthorId = authThomas.AuthorId });
            if (csCat != null) bookCategories.Add(new BookCategory { BookId = pragmatic.BookId, CategoryId = csCat.CategoryId });
            if (seCat != null) bookCategories.Add(new BookCategory { BookId = pragmatic.BookId, CategoryId = seCat.CategoryId });

            // Refactoring
            if (authFowler != null) bookAuthors.Add(new BookAuthor { BookId = refactoring.BookId, AuthorId = authFowler.AuthorId });
            if (seCat != null) bookCategories.Add(new BookCategory { BookId = refactoring.BookId, CategoryId = seCat.CategoryId });

            // DDD
            if (authEvans != null) bookAuthors.Add(new BookAuthor { BookId = ddd.BookId, AuthorId = authEvans.AuthorId });
            if (seCat != null) bookCategories.Add(new BookCategory { BookId = ddd.BookId, CategoryId = seCat.CategoryId });

            // C# in Depth
            if (authSkeet != null) bookAuthors.Add(new BookAuthor { BookId = csharp.BookId, AuthorId = authSkeet.AuthorId });
            if (csCat != null) bookCategories.Add(new BookCategory { BookId = csharp.BookId, CategoryId = csCat.CategoryId });

            // Code Complete
            if (authMcConnell != null) bookAuthors.Add(new BookAuthor { BookId = codeComplete.BookId, AuthorId = authMcConnell.AuthorId });
            if (seCat != null) bookCategories.Add(new BookCategory { BookId = codeComplete.BookId, CategoryId = seCat.CategoryId });

            // TAOCP
            if (authKnuth != null) bookAuthors.Add(new BookAuthor { BookId = taocp.BookId, AuthorId = authKnuth.AuthorId });
            if (csCat != null) bookCategories.Add(new BookCategory { BookId = taocp.BookId, CategoryId = csCat.CategoryId });
            if (mathCat != null) bookCategories.Add(new BookCategory { BookId = taocp.BookId, CategoryId = mathCat.CategoryId });

            await db.BookAuthors.AddRangeAsync(bookAuthors);
            await db.BookCategories.AddRangeAsync(bookCategories);
            await db.SaveChangesAsync();
        }
    }
}
