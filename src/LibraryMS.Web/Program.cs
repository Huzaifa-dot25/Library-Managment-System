using LibraryMS.Application.Interfaces;
using LibraryMS.Infrastructure.Data;
using LibraryMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly("LibraryMS.Infrastructure")
    ));

// ── Application Services ────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthorService,    AuthorService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<ICategoryService,  CategoryService>();
builder.Services.AddScoped<IBookTypeService,  BookTypeService>();
builder.Services.AddScoped<IVendorService,    VendorService>();
builder.Services.AddScoped<IBookService,      BookService>();
builder.Services.AddScoped<IPurchaseService,  PurchaseService>();
builder.Services.AddScoped<IMemberService,    MemberService>();

// ── MVC ─────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Pipeline ─────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
