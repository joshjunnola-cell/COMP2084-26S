/*
 * ============================================================================
 * DotNet Bookstore — In-Class ASP.NET Core MVC Project
 * Course: COMP2084 | Georgian College | Summer 2026
 * Instructor: Dario Hesami
 * ============================================================================
 *
 * FILE:    Models/Category.cs
 * PURPOSE: Defines the Category entity — a simple lookup table that groups
 *          books into genres (e.g., "Fiction", "Science", "Technology").
 *          This is the "one" side of the one-to-many Category → Books relation.
 *
 * KEY CONCEPTS FOR STUDENTS:
 *  1. Model (POCO class)   — a Plain Old C# Object that represents a real-world
 *                            concept. EF Core maps each property to a database
 *                            column and each instance to a table row.
 *  2. Primary Key (PK)     — EF Core recognises "CategoryId" (class name + "Id")
 *                            as the primary key by convention, so no [Key]
 *                            attribute is required.
 *  3. Data Annotations     — attributes like [Required] and [MaxLength] serve
 *                            DUAL purposes: they generate database constraints
 *                            (NOT NULL, VARCHAR(100)) AND drive client/server-
 *                            side validation in forms automatically.
 *  4. Navigation property  — a property that returns a related entity collection
 *                            (Books below). EF Core uses it to JOIN tables; it
 *                            is NOT a database column itself.
 *  5. string.Empty default — initialising string properties to string.Empty
 *                            avoids null-reference warnings in nullable-enabled
 *                            C# projects (.NET 6+).
 * ============================================================================
 */

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DotNetBookstore.Models
{
    public class Category
    {
        // ── Primary Key ──────────────────────────────────────────────────────
        // EF Core convention: a property named "<ClassName>Id" is automatically
        // treated as the Primary Key (auto-incremented integer in SQL Server).
        // Each Category row in the database gets a unique integer ID.
        // Naming tip: you can also use just "Id", but "<ClassName>Id" is more
        // explicit and avoids ambiguity when reading code or writing LINQ queries.
        public int CategoryId { get; set; }

        // ── Name property ────────────────────────────────────────────────────
        // [Required]     → NOT NULL in the database; also triggers a validation
        //                  error if the form is submitted empty.
        // [DisplayName]  → The friendly label shown on forms and table headers
        //                  instead of the raw property name "Name".
        // string.Empty   → Default value prevents null-reference issues at runtime.
        [Required(ErrorMessage = "The category name is required.")]
        [DisplayName("Category Name")]
        public string Name { get; set; } = string.Empty;

        // ── Navigation Property (one-to-many) ────────────────────────────────
        // A Category can have MANY Books associated with it.
        // This collection is NOT stored in the Categories table; it tells EF
        // how to JOIN the Books table when you call .Include(c => c.Books).
        // It is initialised to an empty list so it is never null before data loads.
        // Note: this is the inverse of Book.Category — together they describe
        // the full relationship from both sides.
        public ICollection<Book> Books { get; set; } = [];
    }
}
