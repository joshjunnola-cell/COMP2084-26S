/*
 * ============================================================================
 * DotNet Bookstore — In-Class ASP.NET Core MVC Project
 * Course: COMP2084 | Georgian College | Summer 2026
 * Instructor: Dario Hesami
 * ============================================================================
 *
 * FILE:    Program.cs
 * PURPOSE: Application entry point — the very first code that runs when the
 *          web server starts. It does two main jobs:
 *            1) REGISTER SERVICES into the Dependency Injection (DI) container
 *               (everything added to "builder.Services")
 *            2) BUILD the HTTP REQUEST PIPELINE by chaining middleware
 *               (everything added to "app")
 *
 * KEY CONCEPTS FOR STUDENTS:
 *  1. WebApplicationBuilder  — replaces Startup.cs (introduced in .NET 6).
 *                              Combines configuration, logging, and DI setup.
 *  2. Dependency Injection   — services are registered here once and then
 *                              "injected" automatically wherever needed
 *                              (e.g., controllers receive ApplicationDbContext
 *                              through their constructors).
 *  3. Entity Framework Core  — ORM that translates C# objects to SQL tables.
 *                              Registered via AddDbContext<T>.
 *  4. ASP.NET Core Identity  — built-in user authentication & management.
 *                              Provides Register / Login / Logout pages.
 *  5. Middleware pipeline     — every HTTP request passes through each
 *                              middleware IN ORDER. Order matters!
 *  6. Convention routing     — the pattern "{controller=Home}/{action=Index}/{id?}"
 *                              maps URLs like /Books/Edit/3 automatically.
 * ============================================================================
 */

using DotNetBookstore.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// ── Step 1: Create the WebApplication builder ───────────────────────────────
// WebApplication.CreateBuilder() reads appsettings.json, environment variables,
// command-line args, and sets up logging. The returned "builder" object lets us
// register services BEFORE the app is built.
var builder = WebApplication.CreateBuilder(args);

// ── Step 2: Register Entity Framework Core ──────────────────────────────────
// Read the "DefaultConnection" connection string from appsettings.json.
// The ?? operator throws an exception if the key is missing — it's better to
// fail loudly at startup than silently at runtime when the first DB query runs.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register ApplicationDbContext as a scoped service (one instance per HTTP request).
// UseSqlServer() tells EF Core which database provider to use and passes the
// connection string so EF knows where the database lives.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// In Development mode only: shows a helpful migration error page in the browser
// if the database schema is out of date (instead of a cryptic SQL error).
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ── Step 3: Register ASP.NET Core Identity ───────────────────────────────────
// Identity adds user authentication (register, login, logout, password hashing).
// - AddDefaultIdentity<IdentityUser>() sets up the standard IdentityUser model.
// - RequireConfirmedAccount = true means users must verify their email before logging in.
// - AddEntityFrameworkStores<>() tells Identity to store user data in our SQL database
//   using our ApplicationDbContext (which inherits from IdentityDbContext).
// Use AddIdentity when you need role support. AddDefaultIdentity does not expose all
// the configuration hooks required for adding roles in some project templates, so
// switch to AddIdentity<IdentityUser, IdentityRole>() and enable the default UI
// and token providers so the Identity UI (Razor Pages) continues to work.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// ── Step 4: Register MVC with Views ──────────────────────────────────────────
// This single call registers all the services needed for MVC Controllers + Razor Views.
// Without this, the app would not know how to handle controller routes or render views.
builder.Services.AddControllersWithViews();

// Enable server-side session support (used for anonymous cart id and item count)
// Configure sensible defaults for the demo: 20 minute idle timeout, HttpOnly cookie,
// and mark the cookie as essential so it works even when the user hasn't consented
// to non-essential cookies (useful in demo environments).
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Google OAuth authentication (reads client id/secret from appsettings.json)
builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

// ── Step 5: Build the application ────────────────────────────────────────────
// After this line, no more services can be registered. We switch from setup mode
// to pipeline configuration mode.
var app = builder.Build();

// ── Step 6: Configure the HTTP request pipeline (Middleware) ─────────────────
// IMPORTANT: Middleware runs IN THE ORDER it is added here. Each piece of
// middleware can decide to pass the request to the next one, or short-circuit
// the pipeline and return a response immediately.

if (app.Environment.IsDevelopment())
{
    // Development only: redirect to the EF Core migration page if DB is outdated.
    // This is convenient during development but should NEVER run in production.
    app.UseMigrationsEndPoint();
}
else
{
    // Production: catch unhandled exceptions and redirect to /Home/Error gracefully
    // instead of showing a yellow "server error" screen to users.
    app.UseExceptionHandler("/Home/Error");

    // HSTS (HTTP Strict Transport Security): tells browsers to always use HTTPS.
    // Default is 30 days. Increase for production. See https://aka.ms/aspnetcore-hsts
    app.UseHsts();
}

// Redirect any plain http:// request to https:// automatically.
app.UseHttpsRedirection();

// UseStaticFiles() serves all files from wwwroot/ — including dynamically
// uploaded images (e.g., book cover files saved to wwwroot/img/books/ at runtime).
// It must come BEFORE UseRouting() so static file requests are short-circuited
// before the MVC routing layer runs.
app.UseStaticFiles();

// Set up URL routing so the framework knows which controller/action to invoke.
// Must be called BEFORE UseAuthorization.
app.UseRouting();

// Enable authentication (required when Identity is registered) and session middleware.
// Order: UseAuthentication -> UseSession -> UseAuthorization
app.UseAuthentication();
app.UseSession();

// Check [Authorize] attributes on controllers/actions.
// Must come AFTER UseRouting() and BEFORE MapControllerRoute().
app.UseAuthorization();

// MapStaticAssets() is the .NET 10 optimised companion to UseStaticFiles().
// It handles build-time known assets with content fingerprinting and
// compression, while UseStaticFiles() above handles runtime-uploaded files.
app.MapStaticAssets();

// ── Step 7: Define the default MVC route ─────────────────────────────────────
// Convention-based routing: maps URLs to controllers and actions.
// Pattern: /{controller}/{action}/{id?}
// Defaults: if controller is missing → Home, if action is missing → Index
// Examples:
//   /               → HomeController.Index()
//   /Books          → BooksController.Index()
//   /Books/Edit/3   → BooksController.Edit(3)
//   /Categories/Delete/7 → CategoriesController.Delete(7)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();   // Enables fingerprinted static asset URLs for this route

// Map Razor Pages (used exclusively by ASP.NET Core Identity's built-in UI:
// /Identity/Account/Login, /Identity/Account/Register, etc.)
app.MapRazorPages()
   .WithStaticAssets();

// ── Step 8: Start the web server ─────────────────────────────────────────────
// app.Run() starts listening for HTTP requests and blocks until the app shuts down.
app.Run();
