/*
 * ============================================================================
 * DotNet Bookstore — In-Class ASP.NET Core MVC Project
 * Course: COMP2084 | Georgian College | Summer 2026
 * Instructor: Dario Hesami
 * ============================================================================
 *
 * FILE:    Models/Order.cs
 * PURPOSE: Represents the "header" of a placed order — who ordered, when,
 *          how much in total, and where to ship. The individual line items
 *          (which books and how many) live in the OrderDetail table.
 *
 *          This is the "one" side of the Order → OrderDetails one-to-many
 *          relationship (an order contains many detail lines).
 *
 * KEY CONCEPTS FOR STUDENTS:
 *  1. Order header / detail pattern — splitting order data into two tables is
 *                                     a classic e-commerce database design:
 *                                     Orders holds customer/shipping info once,
 *                                     OrderDetails repeats one row per book.
 *  2. DateTime                      — .NET's built-in type for dates + times.
 *                                     Stored as a SQL DATETIME column. Always
 *                                     set at the time of checkout, not by the user.
 *  3. decimal for money             — always use decimal (not float/double) for
 *                                     currency values. Floating-point types have
 *                                     rounding errors; decimal is exact.
 *  4. [MaxLength] on address fields — keeps the database columns sized correctly
 *                                     and prevents unusually long strings.
 *  5. Navigation property           — OrderDetails is a collection because one
 *                                     order can have many detail rows.
 * ============================================================================
 */

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DotNetBookstore.Models
{
    public class Order
    {
        // ── Primary Key ──────────────────────────────────────────────────────
        // Auto-incremented; uniquely identifies each order.
        public int OrderId { get; set; }

        // ── Timestamp ────────────────────────────────────────────────────────
        // Set programmatically at checkout (e.g., DateTime.Now), never entered
        // manually by the customer. Displayed on order confirmation/history pages.
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }

        // ── Total Amount ─────────────────────────────────────────────────────
        // Calculated by summing (CartItem.Price × CartItem.Quantity) for all
        // items at checkout and stored here for fast retrieval without recalculation.
        [Display(Name = "Order Total")]
        public decimal OrderTotal { get; set; }

        // ── Shipping / Customer Information ───────────────────────────────────
        // These fields capture shipping details at the time of order placement.
        // Storing them here (rather than just using the customer's profile) ensures
        // historical accuracy — if the customer changes their address later, past
        // orders still show the correct delivery address.

        [Required]
        [Display(Name = "First Name")]
        [MaxLength(50, ErrorMessage = "The first name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(50, ErrorMessage = "The last name cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150, ErrorMessage = "The address cannot exceed 150 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(50, ErrorMessage = "The city cannot exceed 50 characters.")]
        public string City { get; set; } = string.Empty;

        // Two-letter province/state code (e.g., "ON" for Ontario)
        [Required]
        [MaxLength(2, ErrorMessage = "The province cannot exceed 2 characters.")]
        public string Province { get; set; } = string.Empty;

        // Canadian postal code format: A1B 2C3 (max 7 chars with space; set to 10 for flexibility)
        [Required]
        [MaxLength(10, ErrorMessage = "The postal code cannot exceed 10 characters.")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(15, ErrorMessage = "The phone number cannot exceed 15 characters.")]
        public string Phone { get; set; } = string.Empty;

        // ── Customer Identifier ───────────────────────────────────────────────
        // Named "CustomerId" but stores the email address of the logged-in user
        // (sourced from User.Identity.Name). Intentionally named to match the
        // same pattern used in CartItem.CustomerId for consistency.
        [Required]
        [Display(Name = "Email")]
        [MaxLength(100, ErrorMessage = "The email cannot exceed 100 characters.")]
        public string CustomerId { get; set; } = string.Empty;

        // ── Navigation Property: OrderDetails (one-to-many) ──────────────────
        // An order contains many line items (one per book purchased).
        // EF Core populates this collection when you call .Include(o => o.OrderDetails).
        // [ValidateNever] prevents the model binder from validating this during POST.
        [ValidateNever]
        public ICollection<OrderDetail> OrderDetails { get; set; } = [];
    }
}
