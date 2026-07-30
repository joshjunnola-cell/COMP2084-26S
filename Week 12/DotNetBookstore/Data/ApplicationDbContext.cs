/*
 * ============================================================================
 * DotNet Bookstore — In-Class ASP.NET Core MVC Project
 * Course: COMP2084 | Georgian College | Summer 2026
 * Instructor: Dario Hesami
 * ============================================================================
 *
 * FILE:    Data/ApplicationDbContext.cs
 * PURPOSE: The Entity Framework Core "Database Context" — the central class
 *          that manages the connection to the SQL Server database and exposes
 *          each database table as a C# DbSet<T> collection.
 *
 * KEY CONCEPTS FOR STUDENTS:
 *  1. DbContext          — EF Core's "unit of work". It tracks changes to
 *                          entities and translates LINQ queries into SQL.
 *  2. IdentityDbContext  — a specialised DbContext provided by ASP.NET Core
 *                          Identity. It already contains DbSets for users,
 *                          roles, claims, etc. We inherit from it so we get
 *                          Identity tables AND our own tables in one database.
 *  3. DbSet<T>           — represents a single SQL table. You query it with
 *                          LINQ (e.g. _context.Books.ToListAsync()) and EF
 *                          generates the matching SELECT statement.
 *  4. Constructor injection — DbContextOptions<T> is injected by the DI
 *                          container (configured in Program.cs) and carries
 *                          the connection string + SQL provider settings.
 *  5. Migrations         — the /Data/Migrations folder contains the history
 *                          of schema changes. Run "dotnet ef migrations add"
 *                          to snapshot model changes, then "dotnet ef database
 *                          update" to apply them to the actual database.
 * ============================================================================
 */

using DotNetBookstore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotNetBookstore.Data
{
    // Inheriting from IdentityDbContext automatically adds all the ASP.NET Core
    // Identity tables (AspNetUsers, AspNetRoles, AspNetUserClaims, etc.) to the
    // database alongside our own application tables.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // The constructor receives DbContextOptions<ApplicationDbContext> which
        // was registered in Program.cs via builder.Services.AddDbContext<>().
        // We pass it straight to the base IdentityDbContext constructor so EF
        // knows which database to connect to.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // ── DbSets (= Database Tables) ───────────────────────────────────────
        // Each DbSet<T> property maps to a SQL table. EF Core uses the PROPERTY
        // NAME (plural by convention) as the table name — e.g. DbSet<Category>
        // named "Categories" → table dbo.Categories.
        //
        // "default!" at the end suppresses the nullable warning: EF Core always
        // initialises DbSet properties when the context is instantiated, so they
        // are never actually null at runtime — but the compiler doesn't know that.

        // Table: Categories — stores book genre/category names (e.g., "Fiction")
        public DbSet<Category> Categories
        {
            get; set;
        } = default!;

        // Table: Books — stores every book in the catalogue, including a FK to
        // the Categories table (CategoryId) to establish the one-to-many relation.
        public DbSet<Book> Books
        {
            get; set;
        } = default!;

        // Table: CartItems — stores shopping cart entries keyed by CustomerId
        // (the logged-in user's email). One user can have many cart items.
        public DbSet<CartItem> CartItems
        {
            get; set;
        } = default!;

        // Table: Orders — stores completed order header information
        // (customer contact details, totals, timestamps).
        public DbSet<Order> Orders
        {
            get; set;
        } = default!;

        // Table: OrderDetails — stores individual line items for each order
        // (which book, how many, at what price). This is the "many" side of
        // the Order → OrderDetails one-to-many relationship.
        public DbSet<OrderDetail> OrderDetails
        {
            get; set;
        } = default!;

        /*
         * ── OPTIONAL: Fluent API Configuration ──────────────────────────────
         * If you ever need to override EF Core conventions, you can use the
         * Fluent API inside OnModelCreating(). Common uses:
         *   - Seed initial data with HasData()
         *   - Define composite primary keys
         *   - Set up unique constraints
         *   - Configure cascade-delete behaviour
         *   - Override table/column names
         *
         * In this project all relationships are fully expressed through:
         *   - Navigation properties (e.g. Book.Category)
         *   - Foreign key properties (e.g. Book.CategoryId)
         *   - Data annotation attributes (e.g. [Required], [ForeignKey])
         * So OnModelCreating is NOT needed right now. It is shown here as
         * a reference for when you extend the project.
         *
         * EXAMPLE (uncomment and modify if needed):
         *
         * protected override void OnModelCreating(ModelBuilder modelBuilder)
         * {
         *     base.OnModelCreating(modelBuilder); // Always call base first for Identity tables
         *
         *     // Seed Categories table with initial data
         *     modelBuilder.Entity<Category>().HasData(
         *         new Category { CategoryId = 1, Name = "Fiction" },
         *         new Category { CategoryId = 2, Name = "Non-Fiction" },
         *         new Category { CategoryId = 3, Name = "Science" },
         *         new Category { CategoryId = 4, Name = "Technology" },
         *         new Category { CategoryId = 5, Name = "Children" }
         *     );
         *
         *     // Explicitly define the Book → Category relationship (optional here)
         *     modelBuilder.Entity<Book>()
         *         .HasOne(b => b.Category)          // A Book has ONE Category
         *         .WithMany(c => c.Books)            // A Category has MANY Books
         *         .HasForeignKey(b => b.CategoryId)  // FK column = CategoryId
         *         .IsRequired();                     // FK is NOT NULL in the database
         * }
         */
    }
}