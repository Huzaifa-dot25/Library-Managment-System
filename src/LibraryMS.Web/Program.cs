using LibraryMS.Application.Interfaces;
using LibraryMS.Infrastructure.Data;
using LibraryMS.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly("LibraryMS.Infrastructure")
    ));

// ── ASP.NET Core Identity ────────────────────────────────────────────────────
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Relaxed password policy for a school library system
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 6;
    options.Password.RequireUppercase       = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase       = false;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<LibraryDbContext>()
.AddDefaultTokenProviders();

// ── Cookie / Login redirect ───────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Account/Login";
    options.LogoutPath       = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan   = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly  = true;
    options.Cookie.IsEssential = true;
});

// ── Application Services ────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthorService,    AuthorService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<ICategoryService,  CategoryService>();
builder.Services.AddScoped<IBookTypeService,  BookTypeService>();
builder.Services.AddScoped<IVendorService,    VendorService>();
builder.Services.AddScoped<IBookService,      BookService>();
builder.Services.AddScoped<IPurchaseService,  PurchaseService>();
builder.Services.AddScoped<IMemberService,    MemberService>();
builder.Services.AddScoped<IIssueService,     IssueService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ── MVC ─────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    // Global auth filter — every controller requires login by default
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
});

var app = builder.Build();

// ── Seed database (roles + default users + lookup data) ─────────────────────
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

// ── Pipeline ─────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();   // must come before UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
