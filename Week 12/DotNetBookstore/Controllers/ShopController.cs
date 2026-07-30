using DotNetBookstore.Data;
using DotNetBookstore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotNetBookstore.Controllers
{
    /*
     * ============================================================================
     * FILE:    Controllers/ShopController.cs
     * PURPOSE: Controller that implements the public shopping surface for the demo
     *          application. It provides actions to list categories, show books by
     *          category, and manage a simple shopping cart (add, update, remove,
     *          and view cart). The implementation is intentionally straightforward
     *          and synchronous-friendly so students can read and understand the
     *          typical ASP.NET Core MVC patterns: DI, EF Core queries, TempData,
     *          sessions, and POST-Redirect-GET for state-changing actions.
     *
     * NOTES:
     *  - Cart ownership is tracked with a session-backed CustomerId (GetCustomerId()).
     *  - Quantity validation is performed both client- and server-side; server
     *    clamps values to a reasonable range.
     *  - TempData is used to provide one-time toast notifications after actions.
     * ============================================================================
     */
    /// <summary>
    /// Public shop surface where users can browse categories and begin shopping.
    /// </summary>
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        // The DbContext is injected via constructor DI. In ASP.NET Core the
        // ApplicationDbContext is usually registered in Program.cs and manages
        // EF Core change-tracking, queries, and migrations. Keeping the
        // controller focused on orchestration (not data access details) makes
        // it easier to test and reason about in examples.
        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // NOTE FOR STUDENTS:
        // The controller receives ApplicationDbContext via Dependency Injection (DI).
        // The DI container creates one context per request (scoped lifetime), which
        // is why we store it in a private readonly field and reuse it in action
        // methods. Do NOT cache DbContext across requests in real apps.

        // GET: /Shop or /Shop/Index
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // GET: /Shop/ShopByCategory/5
        public async Task<IActionResult> ShopByCategory(int? categoryId)
        {
            // Validate incoming route/query parameter early and return a 400 Bad Request
            // if the caller did not provide a category id. This keeps the action
            // expectations explicit and avoids null-reference issues below.
            if (categoryId == null)
            {
                return BadRequest();
            }

            // Fetch the category metadata (name, description). We use FirstOrDefault
            // instead of FindAsync because we are filtering by a nullable int? value
            // coming from model binding; semantics are equivalent here for demo code.
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
            {
                return NotFound();
            }

            // Query books in the requested category and include the Category
            // navigation property so the view can display the category name
            // without executing additional database queries. We also apply a
            // predictable sort (author, then title) so results are stable.
            var books = await _context.Books
                .Where(b => b.CategoryId == categoryId)
                .Include(b => b.Category)
                .OrderBy(b => b.Author)
                .ThenBy(b => b.Title)
                .ToListAsync();

            ViewData["Title"] = $"Shop - {category.Name}";
            ViewData["CategoryName"] = category.Name;

            return View("ShopByCategory", books);
        }

        // POST: /Shop/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
        {
            // Server-side validation: ensure quantity is within a sensible range.
            // Client-side controls help, but server must always validate input.
            if (quantity < 1) quantity = 1;
            if (quantity > 1000) quantity = 1000;

            // Load the Book so we can capture the current price and use the title
            // for user-facing messages. Using FindAsync uses the primary key.
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                // If the book doesn't exist, return 404 to the caller.
                return NotFound();
            }

            // Determine which cart belongs to the current visitor. For the demo
            // we store an identifier in Session (GetCustomerId). In real systems
            // this might be a user id or a more robust cart service.
            var customerId = GetCustomerId();

            // Try to find an existing cart item for this book/customer pair so we
            // can merge quantities rather than creating duplicate rows.
            var cartItem = await _context.CartItems
                .SingleOrDefaultAsync(c => c.BookId == bookId && c.CustomerId == customerId);

            if (cartItem == null)
            {
                // No existing row: create a fresh CartItem capturing the price
                // at the time of adding. Capturing price prevents later admin
                // price edits from changing historical cart values.
                cartItem = new CartItem
                {
                    BookId = bookId,
                    Quantity = quantity,
                    Price = book.Price,
                    CustomerId = customerId
                };

                _context.CartItems.Add(cartItem);
            }
            else
            {
                // Merge quantities: simple additive behavior keeps the demo logic
                // easy to understand. We clamp at save to avoid unrealistic values.
                cartItem.Quantity += quantity;
                _context.CartItems.Update(cartItem);
            }

            // Persist changes to the database. This executes INSERT or UPDATE SQL
            // depending on EF Core change tracker state.
            await _context.SaveChangesAsync();

            // TempData notification for UI toast
            try
            {
                TempData["CartMessage"] = $"Added {quantity} × \"{(book?.Title ?? "item")}\" to your cart.";
                TempData["CartMessageType"] = "success";
            }
            catch { }

            // Update session ItemCount so navbar or layout can show updated count
            var items = await _context.CartItems.Where(c => c.CustomerId == customerId).ToListAsync();
            var itemCount = items.Sum(c => c.Quantity);
            HttpContext.Session.SetInt32("ItemCount", itemCount);

            // Redirect user to Cart page to view their cart
            return RedirectToAction("Cart");
        }

        // identify customer for each shopping cart using session
        private string GetCustomerId()
        {
            // We store a simple CustomerId in session so cart rows can be tied to
            // the browser session. If the user is authenticated we prefer to use
            // their username/email as the identifier so carts survive across
            // browsers (for demo this is good enough). For anonymous visitors we
            // create a GUID and persist it for the session lifetime.
            var sid = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(sid))
            {
                if (User?.Identity?.IsAuthenticated == true)
                {
                    // Use the authenticated user's name (usually email) as id.
                    sid = User.Identity.Name ?? Guid.NewGuid().ToString();
                }
                else
                {
                    // Anonymous visitor: create a GUID to identify their cart.
                    sid = Guid.NewGuid().ToString();
                }

                // Persist the computed id in session for subsequent requests.
                HttpContext.Session.SetString("CustomerId", sid);
            }

            return sid;
        }

        // Small helper to produce a safe, user-friendly title for UI messages
        private static string DisplayTitle(string? title)
        {
            // Normalize null/empty titles to a safe fallback. This avoids
            // displaying raw null values in UI messages. We also strip simple
            // placeholder prefixes used by sample data so messages look cleaner
            // for students exploring the app.
            if (string.IsNullOrWhiteSpace(title)) return "item";
            var t = title.Trim();
            if (t.StartsWith("X-", StringComparison.OrdinalIgnoreCase)) t = t.Substring(2).Trim();
            return t;
        }

        // GET: /Shop/Cart
        public async Task<IActionResult> Cart()
        {
            // Read/ensure customer id for the current session (creates one if needed)
            var customerId = GetCustomerId();

            // Load cart items for the current customer and include the related
            // Book entities so the view can render title, image, and author
            // without issuing additional database queries.
            var cartItems = await _context.CartItems
                .Include(c => c.Book)
                .Where(c => c.CustomerId == customerId)
                .OrderBy(c => c.Book.Title)
                .ToListAsync();

            // Maintain a session-based counter of total items for display in the
            // site header (e.g., navbar badge). This demonstrates using session
            // to share small pieces of state across requests.
            var itemCount = cartItems.Sum(c => c.Quantity);
            HttpContext.Session.SetInt32("ItemCount", itemCount);

            // Render the Cart view passing the list of CartItem objects as the model.
            return View(cartItems);
        }

        // POST: /Shop/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            // Remove a cart item. This is an intentionally simple POST action
            // protected with [ValidateAntiForgeryToken]. Always use POST for
            // state-changing operations (avoid GET for deletes).

            // Load the cart item including its Book so we can show a friendly
            // message containing the book's title after the delete.
            var cartItem = await _context.CartItems
                .Include(c => c.Book)
                .FirstOrDefaultAsync(c => c.CartItemId == id);

            if (cartItem != null)
            {
                // Keep a local copy of the title for the notification before
                // we delete the database row.
                var title = cartItem.Book?.Title ?? "item";

                // Mark the entity for deletion and persist changes.
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                // Refresh the session-based item count so the navbar badge stays
                // in sync with the persisted cart contents.
                var customerId = GetCustomerId();
                var items = await _context.CartItems.Where(c => c.CustomerId == customerId).ToListAsync();
                HttpContext.Session.SetInt32("ItemCount", items.Sum(c => c.Quantity));

                // Use TempData to send a one-time message to the next request so
                // the layout can render a toast notification (Post-Redirect-Get).
                try
                {
                    TempData["CartMessage"] = $"Removed \"{title}\" from your cart.";
                    TempData["CartMessageType"] = "warning";
                }
                catch { }
            }

            return RedirectToAction("Cart");
        }

        // POST: /Shop/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            // Server-side validation: clamp incoming quantity to sensible bounds.
            // Client-side controls help, but the server must always validate input.
            if (quantity < 1) quantity = 1;
            if (quantity > 1000) quantity = 1000;

            // Load the CartItem and include the related Book navigation property.
            // Including the Book here avoids issuing a separate query later when
            // composing the user-facing message and makes the data access flow
            // explicit for educational purposes.
            var cartItem = await _context.CartItems
                .Include(c => c.Book)
                .FirstOrDefaultAsync(c => c.CartItemId == cartItemId);

            if (cartItem == null)
            {
                // If the cart item doesn't exist (removed elsewhere or invalid id),
                // return 404 so the caller can handle the missing resource.
                return NotFound();
            }

            // Update the entity and persist changes. This demonstrates a simple
            // edit flow: modify the tracked entity, call Update (explicit here
            // for clarity), then SaveChangesAsync to flush SQL to the database.
            cartItem.Quantity = quantity;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();

            // Recalculate the total item count for the current customer and store
            // it in session so shared UI (e.g., navbar badge) stays in sync.
            var customerId = GetCustomerId();
            var items = await _context.CartItems.Where(c => c.CustomerId == customerId).ToListAsync();
            HttpContext.Session.SetInt32("ItemCount", items.Sum(c => c.Quantity));

            // Use TempData to provide a one-time informational message on the next
            // GET request (follows the Post-Redirect-Get pattern). This is a
            // lightweight way to surface a toast confirming the update.
            try
            {
                var title = DisplayTitle(cartItem.Book?.Title);
                TempData["CartMessage"] = $"Updated quantity for \"{title}\" — now {quantity} in your cart.";
                TempData["CartMessageType"] = "info";
            }
            catch { }

            return RedirectToAction("Cart");
        }
    }
}