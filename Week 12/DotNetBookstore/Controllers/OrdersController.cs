using DotNetBookstore.Data;
using DotNetBookstore.Models;
using DotNetBookstore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace DotNetBookstore.Controllers
{
    /// <summary>
    /// Handles checkout, order confirmation and history.
    /// This is intentionally simple for teaching purposes.
    /// </summary>
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public OrdersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: /Orders/Checkout
        // Displays a simple shipping form and order summary before redirecting
        // the user to Stripe Checkout.
        public async Task<IActionResult> Checkout()
        {
            // Ensure customer id (matches ShopController's approach)
            var customerId = GetCustomerId();

            // Load cart items for the current customer
            var cartItems = await _context.CartItems
                .Include(c => c.Book)
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();

            if (cartItems == null || !cartItems.Any())
            {
                TempData["CartMessage"] = "Your cart is empty. Add items before checkout.";
                return RedirectToAction("Cart", "Shop");
            }

            // Prefill a lightweight Order object for the view. Do NOT save yet.
            var model = new Order
            {
                OrderDate = DateTime.Now,
                OrderTotal = cartItems.Sum(i => i.Price * i.Quantity),
                CustomerId = User?.Identity?.IsAuthenticated == true ? User.Identity.Name ?? customerId : customerId
            };

            // If the user previously started checkout and we saved a pending order
            // in session, prefill the form so they can resume without retyping.
            try
            {
                var pending = HttpContext.Session.GetObject<Order>("PendingOrder");
                if (pending != null)
                {
                    // Copy user-entered shipping fields from the pending order
                    model.FirstName = pending.FirstName;
                    model.LastName = pending.LastName;
                    model.Address = pending.Address;
                    model.City = pending.City;
                    model.Province = pending.Province;
                    model.PostalCode = pending.PostalCode;
                    model.Phone = pending.Phone;
                    model.CustomerId = pending.CustomerId ?? model.CustomerId;
                }
            }
            catch { }

            // Pass both the order and cart items via ViewData for the simple view
            ViewData["CartItems"] = cartItems;
            return View(model);
        }

        // POST: /Orders/Checkout
        // Creates a Stripe Checkout Session and redirects the user to Stripe's
        // hosted payment page. The order is stored temporarily in session so
        // it can be created in the database after successful payment.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            // Basic model validation for required shipping fields
            if (!ModelState.IsValid)
            {
                var cartItems = await _context.CartItems
                    .Include(c => c.Book)
                    .Where(c => c.CustomerId == GetCustomerId())
                    .ToListAsync();

                ViewData["CartItems"] = cartItems;
                return View(order);
            }

            // Build line items from cart and compute order total server-side
            var cart = await _context.CartItems.Include(c => c.Book)
                .Where(c => c.CustomerId == GetCustomerId())
                .ToListAsync();

            // Compute and set OrderTotal on the order before storing it in session.
            // This ensures the server has an authoritative amount independent of
            // any client-side manipulation.
            order.OrderTotal = cart.Sum(i => i.Price * i.Quantity);

            // Save the incoming order (with computed total) temporarily in session (serialized)
            HttpContext.Session.SetObject("PendingOrder", order);

            // Prepare Stripe
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            // Build success/cancel URLs manually to avoid Url.Action encoding the
            // placeholder token {CHECKOUT_SESSION_ID} which would prevent Stripe
            // from substituting the real session id on redirect.
            var origin = $"{Request.Scheme}://{Request.Host.Value}";
            var successUrl = $"{origin}/Orders/Success?session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"{origin}/Orders/Cancel";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                LineItems = new List<SessionLineItemOptions>(),
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            foreach (var item in cart)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // cents
                        Currency = "cad",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Book?.Title ?? "Book",
                        }
                    },
                    Quantity = item.Quantity
                });
            }

            var service = new SessionService();
            var session = service.Create(options);

            // Keep the Stripe checkout id in session for reference (optional)
            HttpContext.Session.SetString("StripeCheckoutId", session.Id);

            // Redirect the user to Stripe's hosted checkout page
            return Redirect(session.Url!);
        }

        // GET: /Orders/Success
        // Called by Stripe after successful payment when using success_url.
        // For a production-ready scenario webhooks are preferred; see docs.
        public async Task<IActionResult> Success(string session_id)
        {
            // Validate input
            if (string.IsNullOrEmpty(session_id))
                return BadRequest();

            // Initialize Stripe
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            var service = new SessionService();
            var session = service.Get(session_id);

            // Simple check: ensure the payment was completed
            if (session.PaymentStatus != "paid")
            {
                // Not paid yet — show a simple warning page
                ViewBag.Message = "Payment not completed. If you were charged, contact support.";
                return View("PaymentPending");
            }

            // Retrieve the pending order from session
            var pending = HttpContext.Session.GetObject<Order>("PendingOrder");
            if (pending == null)
            {
                // No pending order found; this can happen if session expired.
                TempData["CartMessage"] = "Could not find the pending order in session. Contact support.";
                return RedirectToAction("Index", "Home");
            }

            // Build and save the Order and OrderDetails using the saved cart
            var customerId = GetCustomerId();
            var cartItems = await _context.CartItems.Where(c => c.CustomerId == customerId).ToListAsync();

            var order = new Order
            {
                OrderDate = DateTime.Now,
                FirstName = pending.FirstName,
                LastName = pending.LastName,
                Address = pending.Address,
                City = pending.City,
                Province = pending.Province,
                PostalCode = pending.PostalCode,
                Phone = pending.Phone,
                CustomerId = pending.CustomerId,
                OrderTotal = cartItems.Sum(i => i.Price * i.Quantity)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Persist detail lines
            foreach (var ci in cartItems)
            {
                var detail = new OrderDetail
                {
                    BookId = ci.BookId,
                    OrderId = order.OrderId,
                    Price = ci.Price,
                    Quantity = ci.Quantity
                };

                _context.OrderDetails.Add(detail);
            }

            // Remove the saved cart items now that order completed
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            // Clear pending order session and update ItemCount
            HttpContext.Session.Remove("PendingOrder");
            HttpContext.Session.SetInt32("ItemCount", 0);

            // Show confirmation view with the saved order (include details & book)
            var saved = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            return View("Success", saved);
        }

        // GET: /Orders/Cancel
        public IActionResult Cancel()
        {
            // User cancelled at Stripe's checkout
            TempData["CartMessage"] = "Payment cancelled. You can retry checkout anytime.";
            return RedirectToAction("Cart", "Shop");
        }

        // GET: /Orders/History
        public async Task<IActionResult> History()
        {
            var customerId = GetCustomerId();
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Book)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // Duplicate of ShopController's customer id logic to keep the demo simple
        private string GetCustomerId()
        {
            var sid = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(sid))
            {
                if (User?.Identity?.IsAuthenticated == true)
                {
                    sid = User.Identity.Name ?? Guid.NewGuid().ToString();
                }
                else
                {
                    sid = Guid.NewGuid().ToString();
                }

                HttpContext.Session.SetString("CustomerId", sid);
            }

            return sid;
        }
    }
}