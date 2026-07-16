/*
 * ============================================================================
 * DotNet Bookstore — In-Class ASP.NET Core MVC Project
 * Course: COMP2084 | Georgian College | Summer 2026
 * Instructor: Dario Hesami
 * ============================================================================
 *
 * FILE:    Models/OrderDetail.cs
 * PURPOSE: Represents one line item within an order — which specific book was
 *          purchased, how many copies, and at what unit price.
 *
 *          This is the "many" side of TWO relationships:
 *            - Order  → OrderDetails (one order has many detail lines)
 *            - Book   → OrderDetails (one book can appear in many orders)
 *
 * KEY CONCEPTS FOR STUDENTS:
 *  1. Junction / bridge table   — OrderDetail sits between Orders and Books.
 *                                 It records the ASSOCIATION between a specific
 *                                 order and a specific book, enriched with
 *                                 context data (Quantity, Price).
 *  2. Two FKs in one model      — OrderId links to Orders; BookId links to Books.
 *                                 EF Core creates two foreign key constraints in
 *                                 the database from these two properties.
 *  3. Price capture             — same principle as CartItem.Price: we store the
 *                                 book's price at the time of purchase, not a
 *                                 live reference, so historical order totals stay
 *                                 accurate even if book prices change later.
 *  4. [ValidateNever]           — applied to navigation properties so the model
 *                                 binder does not expect them to be submitted
 *                                 as form fields (EF Core resolves them from FK).
 * ============================================================================
 */

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DotNetBookstore.Models
{
    public class OrderDetail
    {
        // ── Primary Key ──────────────────────────────────────────────────────
        // Auto-incremented; uniquely identifies each line item across all orders.
        public int OrderDetailId { get; set; }

        // ── Quantity ─────────────────────────────────────────────────────────
        // Number of copies of this book in the order. At least 1 is required
        // (you can't place an order for 0 books).
        [Required]
        public int Quantity { get; set; }

        // ── Price (captured at time of purchase) ─────────────────────────────
        // Unit price of the book at the time the order was placed.
        // Stored here so the order history is accurate even if the book's
        // current price in the Books table changes later.
        [Required]
        public decimal Price { get; set; }

        // ── Foreign Key: OrderId ──────────────────────────────────────────────
        // Points to the parent Order row. Every line item MUST belong to an
        // order; no orphaned detail rows are allowed (NOT NULL in the database).
        [Required]
        public int OrderId { get; set; }

        // ── Navigation Property: Order ────────────────────────────────────────
        // Allows you to navigate from a detail line back to its parent order,
        // e.g., detail.Order.OrderDate. EF Core populates it via .Include().
        // null! → will never actually be null once loaded by EF Core.
        public Order Order { get; set; } = null!;

        // ── Foreign Key: BookId ───────────────────────────────────────────────
        // Points to the Book that was purchased in this line item.
        [Required]
        public int BookId { get; set; }

        // ── Navigation Property: Book ─────────────────────────────────────────
        // Allows access to full book details (title, author, image) on the order
        // confirmation and history pages via .Include(d => d.Book).
        // [ValidateNever] skips validation for this object during form binding.
        [ValidateNever]
        public Book Book { get; set; } = null!;
    }
}
