/*
 * ============================================================================
 * DotNet Bookstore — In-Class ASP.NET Core MVC Project
 * Course: COMP2084 | Georgian College | Summer 2026
 * Instructor: Dario Hesami
 * ============================================================================
 *
 * FILE:    Models/CartItem.cs
 * PURPOSE: Represents a single item in a user's shopping cart.
 *          Each row links a logged-in customer (by email/CustomerId) to a
 *          specific Book with a chosen quantity and captured price.
 *
 * KEY CONCEPTS FOR STUDENTS:
 *  1. Shopping cart pattern  — instead of storing cart data in a session
 *                              (which disappears when the browser closes),
 *                              we persist it in the database so the cart
 *                              survives page refreshes and re-logins.
 *  2. CustomerId as string   — we use the user's email address as the cart
 *                              owner identifier. This ties directly to
 *                              IdentityUser.Email (from ASP.NET Core Identity).
 *  3. Price capture          — storing the price at the time the item was
 *                              added prevents the cart total from changing if
 *                              an admin later edits the book's price.
 *  4. FK + Navigation        — BookId is the foreign key; Book is the
 *                              navigation property. Both work together to
 *                              let EF Core JOIN the Books table on demand.
 * ============================================================================
 */

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DotNetBookstore.Models
{
    public class CartItem
    {
        // ── Primary Key ──────────────────────────────────────────────────────
        // Auto-incremented integer; uniquely identifies each row in CartItems.
        public int CartItemId { get; set; }

        // ── Quantity ─────────────────────────────────────────────────────────
        // How many copies of the book the customer wants.
        // [Range(1, 1000)] ensures the user can't enter 0 or a negative number,
        // and caps at a reasonable maximum to prevent data issues.
        [Required]
        [Range(1, 1000, ErrorMessage = "Quantity must be at least 1 and realistic.")]
        public int Quantity { get; set; }

        // ── Price (captured at time of adding) ───────────────────────────────
        // Snapshot of the book's price when it was added to the cart.
        // Stored separately from Book.Price so that later price changes don't
        // retroactively alter the cart total.
        [Required]
        [Range(0.01, 10000, ErrorMessage = "The price must be greater than zero and between 0.01 and 10000.")]
        public decimal Price { get; set; }

        // ── Customer Identifier ──────────────────────────────────────────────
        // The email address of the logged-in user who owns this cart item.
        // Sourced from User.Identity.Name (set by ASP.NET Core Identity on login).
        // This is how we filter cart items per user: WHERE CustomerId = @email.
        [Required]
        public string CustomerId { get; set; } = string.Empty;

        // ── Foreign Key: BookId ───────────────────────────────────────────────
        // References the BookId in the Books table. EF Core uses this integer
        // to enforce that you cannot add a CartItem for a non-existent Book.
        [Required]
        public int BookId { get; set; }

        // ── Navigation Property: Book ─────────────────────────────────────────
        // Loaded with .Include(c => c.Book) in the controller to allow the view
        // to access book.Title, book.Author, book.Image, etc. without extra queries.
        // [ValidateNever] prevents form POST validation from requiring this object.
        [ValidateNever]
        public Book Book { get; set; } = null!;
    }
}
