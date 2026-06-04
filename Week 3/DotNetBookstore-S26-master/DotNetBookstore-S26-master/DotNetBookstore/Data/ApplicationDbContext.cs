using DotNetBookstore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotNetBookstore.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets for all entities - these represent the tables in the database
        // Each DbSet property corresponds to a table in the database, and the type parameter (e.g., Category, Book, CartItem, Order, OrderDetail) represents the entity type that will be stored in that table. These properties allow you to query and manipulate data for each of these entities using Entity Framework Core's LINQ queries and other data access methods.
        public DbSet<Category> Categories
        {
            get; set;
        } = default!;

        public DbSet<Book> Books
        {
            get; set;
        } = default!;

        public DbSet<CartItem> CartItems
        {
            get; set;
        } = default!;

        public DbSet<Order> Orders
        {
            get; set;
        } = default!;

        public DbSet<OrderDetail> OrderDetails
        {
            get; set;
        } = default!;

        /*
        // Optional: Fluent API configuration (if needed later)
        // The OnModelCreating method is where you can use the Fluent API to configure the model and its relationships in more detail. This is an alternative to using data annotations directly on the model classes. You can use the Fluent API to define relationships, configure keys, set up constraints, and customize the database schema in ways that may not be possible with data annotations alone.

        // Note:
        // In this current project, it is not strictly necessary to configure relationships in OnModelCreating()
        // because they are already fully defined using data annotations and clear navigation + foreign key properties.
        // This Fluent API setup would be redundant unless you:
        // - Encounter special cases (e.g., composite keys, cascade delete rules, table splitting),
        // - Or prefer to centralize configuration logic outside of model classes.
        // You can safely skip OnModelCreating unless one of those needs arises.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          base.OnModelCreating(modelBuilder);
          // If needed: explicitly define relationships, define composite primary keys, enforce unique constraints or performance tuning, seed the database with initial data, set default values for properties, control cascade delete and referential actions, etc.
          // For example:
          // Seeding initial data for Category
          modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Fiction" },
            new Category { CategoryId = 2, Name = "Non-Fiction" },
            new Category { CategoryId = 3, Name = "Science" },
            new Category { CategoryId = 4, Name = "Technology" },
            new Category { CategoryId = 5, Name = "Children" }
          );
          // Book must have a Category (required FK relationship)
          modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .IsRequired();
        }
        */
    }
}