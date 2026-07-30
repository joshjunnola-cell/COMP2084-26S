/*
 * ============================================================================
 * DotNet Bookstore — In-Class ASP.NET Core MVC Project
 * Course: COMP2084 | Georgian College | Summer 2026
 * Instructor: Dario Hesami
 * ============================================================================
 *
 * FILE:    Controllers/CategoriesController.cs
 * PURPOSE: Implements full CRUD (Create, Read, Update, Delete) for the
 *          Category entity. Each public method is an action that handles one
 *          type of HTTP request and returns a view or a redirect.
 *
 * CRUD → HTTP METHOD MAPPING:
 *  Index          GET  /Categories          → list all categories
 *  Details        GET  /Categories/Details/5 → show one category
 *  Create (GET)   GET  /Categories/Create    → show empty form
 *  Create (POST)  POST /Categories/Create    → save new category
 *  Edit (GET)     GET  /Categories/Edit/5    → show pre-filled form
 *  Edit (POST)    POST /Categories/Edit/5    → save changes
 *  Delete (GET)   GET  /Categories/Delete/5  → show confirmation page
 *  Delete (POST)  POST /Categories/Delete/5  → permanently remove record
 *
 * KEY CONCEPTS FOR STUDENTS:
 *  1. Constructor injection     — ApplicationDbContext is provided by the DI
 *                                 container. We store it in a private readonly
 *                                 field to use throughout the class.
 *  2. async / await             — all database calls are asynchronous (they use
 *                                 the Async suffix). This frees the web thread
 *                                 while waiting for SQL Server to respond,
 *                                 improving scalability under load.
 *  3. [HttpPost]                — decorating an action means it responds ONLY to
 *                                 HTTP POST requests (form submissions). Without
 *                                 it the action responds to GET requests.
 *  4. [ValidateAntiForgeryToken]— prevents CSRF (Cross-Site Request Forgery)
 *                                 attacks by verifying a hidden token that was
 *                                 embedded in the HTML form by ASP.NET Core's
 *                                 asp-action Tag Helper.
 *  5. [Bind("...")]             — restricts which model properties are populated
 *                                 from the POST data, preventing "over-posting"
 *                                 (mass assignment) security attacks.
 *  6. ModelState.IsValid        — true only if all [Required], [Range], etc.
 *                                 validation rules passed. If false, we return
 *                                 the form view so the user can correct errors.
 *  7. DbUpdateConcurrencyException — thrown when two users try to edit the same
 *                                 record simultaneously. We re-check if the record
 *                                 still exists: if it was deleted by another user,
 *                                 return 404; otherwise re-throw.
 * ============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetBookstore.Models;
using DotNetBookstore.Data;
using Microsoft.AspNetCore.Authorization;

// Note: this controller is intentionally in the global namespace (no explicit
// namespace declaration) to match the scaffolded output. In production code
// you would add: namespace DotNetBookstore.Controllers { ... }

// Only users in the "Administrator" role can access any action in this controller.
// If you want allow other roles or to access certain actions, you can override this with [AllowAnonymous] or [Authorize(Roles = "OtherRole")] on individual actions. For example two roles (Administrator and Manager) can be allowed to access the Index action by adding [Authorize(Roles = "Administrator,Manager")] 
[Authorize(Roles = "Administrator")]
public class CategoriesController : Controller
{
    // Private field to hold the database context.
    // "readonly" means it can only be assigned in the constructor — a good
    // practice that makes the dependency explicit and prevents accidental
    // reassignment later in the class.
    private readonly ApplicationDbContext _context;

    // Constructor injection: ASP.NET Core's DI container calls this constructor
    // and automatically passes in the ApplicationDbContext that was registered
    // in Program.cs. We store it in _context for use in all action methods.
    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ── GET: /Categories ─────────────────────────────────────────────────────
    // Fetches ALL categories from the database and passes the list to the view.
    // ToListAsync() executes a SELECT * FROM Categories and materialises the
    // results into a List<Category> asynchronously.
    public async Task<IActionResult> Index()
    {
        return View(await _context.Categories.ToListAsync());
    }

    // ── GET: /Categories/Details/5 ────────────────────────────────────────────
    // Shows the full details of a single category identified by its ID.
    // The "int? id" parameter is nullable so we can detect when the URL has
    // no ID segment at all (e.g., /Categories/Details with nothing after it).
    [AllowAnonymous] // Anyone can view category details, even if not logged in.
    public async Task<IActionResult> Details(int? id)
    {
        // Guard: if no ID was provided in the URL, return HTTP 404 Not Found.
        if (id == null)
        {
            return NotFound();
        }

        // FirstOrDefaultAsync returns the first matching row, or null if not found.
        // The lambda "m => m.CategoryId == id" is the WHERE clause in SQL:
        //   SELECT TOP 1 * FROM Categories WHERE CategoryId = @id
        var category = await _context.Categories
            .FirstOrDefaultAsync(m => m.CategoryId == id);

        // Guard: if no category with this ID exists, return 404.
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // ── GET: /Categories/Create ───────────────────────────────────────────────
    // Returns an empty form for creating a new category.
    // No database query needed — the form starts blank.
    public IActionResult Create()
    {
        return View();
    }

    // ── POST: /Categories/Create ──────────────────────────────────────────────
    // Receives the submitted form data, validates it, and saves the new category.
    //
    // [HttpPost]               — this action ONLY handles form submissions (POST).
    // [ValidateAntiForgeryToken] — checks the hidden __RequestVerificationToken
    //                             field to block CSRF attacks.
    // [Bind("CategoryId,Name")] — only CategoryId and Name are allowed through
    //                             the model binder. Even if a malicious user adds
    //                             extra fields to the POST body, they are ignored.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CategoryId,Name")] Category category)
    {
        // Server-side validation: check all data annotation rules ([Required], etc.)
        // Client-side jQuery validation runs first in the browser, but we ALWAYS
        // validate on the server too — never trust client-side validation alone.
        if (ModelState.IsValid)
        {
            // Add the new entity to the DbContext's change tracker (not yet in DB).
            _context.Add(category);

            // Flush all tracked changes to the database in a single transaction.
            // This generates: INSERT INTO Categories (Name) VALUES (@Name)
            await _context.SaveChangesAsync();

            // After a successful save, redirect to the Index action (PRG pattern:
            // Post/Redirect/Get prevents duplicate submissions on browser refresh).
            return RedirectToAction(nameof(Index));
        }

        // Validation failed — return the form view with the invalid data still
        // populated (and validation error messages displayed) so the user can fix them.
        return View(category);
    }

    // ── GET: /Categories/Edit/5 ───────────────────────────────────────────────
    // Fetches the category with the given ID and returns a pre-filled edit form.
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // FindAsync is optimised for lookup by primary key — it checks the
        // EF Core identity cache first before hitting the database.
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // ── POST: /Categories/Edit/5 ──────────────────────────────────────────────
    // Receives the updated form data and saves changes to the existing category.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("CategoryId,Name")] Category category)
    {
        // Extra safety check: the route ID must match the model's CategoryId.
        // A mismatch would indicate a tampered request.
        if (id != category.CategoryId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Update() marks ALL properties of the entity as "modified" in EF's
                // change tracker. SaveChangesAsync() then generates:
                //   UPDATE Categories SET Name = @Name WHERE CategoryId = @id
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Concurrency conflict: another user changed/deleted this record
                // between when we loaded it and when we tried to save.
                if (!CategoryExists(category.CategoryId))
                {
                    // The record was deleted by someone else — tell the user it's gone.
                    return NotFound();
                }
                else
                {
                    // Unknown concurrency issue — re-throw so the global error handler
                    // logs it and shows the Error page.
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        return View(category);
    }

    // ── GET: /Categories/Delete/5 ─────────────────────────────────────────────
    // Shows a confirmation page before permanently deleting a category.
    // We NEVER delete directly from a GET request — GET must be safe/idempotent.
    // The actual deletion only happens when the user submits the confirmation form.
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(m => m.CategoryId == id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // ── POST: /Categories/Delete/5 ────────────────────────────────────────────
    // Permanently removes the category after the user confirms on the delete page.
    //
    // ActionName("Delete") — maps this POST action to the same URL as the GET
    // Delete action (/Categories/Delete/5). Without this attribute, ASP.NET Core
    // couldn't distinguish the two Delete methods at runtime.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        // Find the category by primary key.
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            // Mark the entity for deletion in EF Core's change tracker.
            _context.Categories.Remove(category);
        }

        // Commit the DELETE SQL command to the database.
        // Note: if the category is referenced by books (FK constraint), this will
        // throw a database exception. Consider adding cascade delete or validating
        // that no books exist in this category before allowing deletion.
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ── Private helper ────────────────────────────────────────────────────────
    // Checks whether a category with the given ID still exists in the database.
    // Used by the Edit POST action to distinguish a concurrency error (record
    // still exists but was changed) from a delete (record is gone).
    private bool CategoryExists(int? id)
    {
        // Any() with a predicate generates: SELECT TOP 1 1 FROM Categories WHERE CategoryId = @id
        // Returns true if at least one matching row exists.
        return _context.Categories.Any(e => e.CategoryId == id);
    }
}