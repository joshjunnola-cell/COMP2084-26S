/*
 * ============================================================================
 * DotNet Bookstore — In-Class ASP.NET Core MVC Project
 * Course: COMP2084 | Georgian College | Summer 2026
 * Instructor: Dario Hesami
 * ============================================================================
 *
 * FILE:    Models/Book.cs
 * PURPOSE: Defines the Book entity — the core domain object of the bookstore.
 *          A Book belongs to exactly one Category (mandatory FK), and can
 *          optionally appear in many CartItems and many OrderDetails.
 *
 * KEY CONCEPTS FOR STUDENTS:
 *  1. Foreign Key (FK)       — CategoryId is a FK that references the PK of
 *                              the Categories table. EF Core uses it to enforce
 *                              referential integrity at the database level.
 *  2. Navigation properties  — Category, CartItems, and OrderDetails are
 *                              navigation properties: they are NOT columns in
 *                              the Books table. EF Core uses them to write JOIN
 *                              queries when you call .Include().
 *  3. [ValidateNever]        — tells ASP.NET Core model validation to SKIP
 *                              these navigation properties during POST requests.
 *                              Without it, the validator would complain that
 *                              Category is null, even though that's expected
 *                              (EF Core fills it, not the HTML form).
 *  4. [Required] + [Range]   — combine to enforce both presence and value range
 *                              in one step, both on the server and in the browser.
 *  5. [DisplayFormat]        — controls how the value is rendered by Html helpers
 *                              (e.g., "{0:c}" formats a decimal as currency).
 *  6. null! (null-forgiving)  — tells the compiler "this will never be null at
 *                              runtime, trust me". Safe to use for navigation
 *                              properties because EF Core always populates them
 *                              when you include them in a query.
 *  7. Image as filename       — Book.Image stores only the GUID-prefixed filename
 *                              of the uploaded cover (e.g., "abc123-cover.jpg"),
 *                              NOT a URL. The full browser path is constructed in
 *                              views as ~/img/books/{Image}. The file is written to
 *                              wwwroot/img/books/ by BooksController.UploadImage()
 *                              and deleted by BooksController.DeleteImage() on
 *                              book update (image replace) or delete.
 * ============================================================================
 */

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DotNetBookstore.Models
{
    public class Book
    {
        // ── Primary Key ──────────────────────────────────────────────────────
        // Auto-incremented by SQL Server (IDENTITY column). Never set manually.
        public int BookId { get; set; }

        // ── Author ───────────────────────────────────────────────────────────
        // [Required]   → must not be empty (NOT NULL in database).
        // [MaxLength]  → limits the column to 100 chars in the DB schema AND
        //                shows a custom error if the form value is too long.
        [Required]
        [MaxLength(100, ErrorMessage = "The author's name cannot exceed 100 characters.")]
        public string Author { get; set; } = string.Empty;

        // ── Title ────────────────────────────────────────────────────────────
        [Required]
        [MaxLength(200, ErrorMessage = "The title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        // ── Image (nullable) ─────────────────────────────────────────────────
        // Stores the GUID-prefixed filename of the uploaded cover image
        // (e.g., "a3f9c12b-...-cover.jpg"), NOT a URL. The file itself is saved
        // to wwwroot/img/books/ by BooksController.UploadImage(). Views build
        // the browser-accessible path as: ~/img/books/{Image}
        //
        // The "?" makes this nullable — a book can exist without a cover image.
        // When null, the views fall back to the gradient placeholder icon.
        // [ValidateNever] is NOT needed here because Image is excluded from
        // [Bind] in the controller and handled separately as an IFormFile.
        public string? Image { get; set; }

        // ── Price ────────────────────────────────────────────────────────────
        // [Range]         → validates the value is between 0.01 and 10000.
        // [DisplayFormat] → formats the decimal as currency (e.g., "$24.99")
        //                   in read-only display helpers (@Html.DisplayFor).
        //                   Does NOT affect the raw <input> in edit forms.
        [Required]
        [Range(0.01, 10000, ErrorMessage = "The price must be greater than zero and between 0.01 and 10000.")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Price { get; set; }

        // ── MatureContent ────────────────────────────────────────────────────
        // A boolean flag rendered as a toggle switch in Create/Edit forms.
        // Defaults to false (all ages) unless explicitly checked.
        // [Display] overrides the property name with a friendlier label.
        [Display(Name = "Mature Content")]
        public bool MatureContent { get; set; }

        // ── Foreign Key: CategoryId ──────────────────────────────────────────
        // This integer column is stored in the Books table and points to the
        // CategoryId column in the Categories table. The database enforces that
        // no book can reference a CategoryId that does not exist in Categories.
        // In the Create/Edit forms, the user picks a category from a <select>
        // dropdown — the selected value is posted as this integer.
        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        // ── Navigation Property: Category (many-to-one) ──────────────────────
        // Allows you to write book.Category.Name instead of doing a manual JOIN.
        // EF Core populates this ONLY when you use .Include(b => b.Category) in
        // the query — otherwise it is null (lazy loading is off by default).
        // [ValidateNever] prevents the MVC model binder from complaining that
        // this object is null when the Create/Edit form is submitted.
        [ValidateNever]
        public Category Category { get; set; } = null!;

        // ── Navigation Property: CartItems (one-to-many) ─────────────────────
        // A book can appear in many users' shopping carts at the same time.
        // Initialised to an empty collection so it is never null before loading.
        [ValidateNever]
        public ICollection<CartItem> CartItems { get; set; } = [];

        // ── Navigation Property: OrderDetails (one-to-many) ──────────────────
        // A book can appear in many different orders (each as a separate detail line).
        [ValidateNever]
        public ICollection<OrderDetail> OrderDetails { get; set; } = [];
    }
}
