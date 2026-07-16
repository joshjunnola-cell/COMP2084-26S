/*
 * ============================================================================
 * DotNet Bookstore — In-Class ASP.NET Core MVC Project
 * Course: COMP2084 | Georgian College | Summer 2026
 * Instructor: Dario Hesami
 * ============================================================================
 *
 * FILE:    Controllers/BooksController.cs
 * PURPOSE: Implements full CRUD (Create, Read, Update, Delete) for the Book
 *          entity. Because a Book belongs to a Category (foreign key) and can
 *          have an uploaded cover image, this controller is more complex than
 *          CategoriesController — it must:
 *            • Eagerly load the related Category for display (.Include())
 *            • Populate a dropdown list for the Category selector (SelectList)
 *            • Re-populate the dropdown on validation failure
 *            • Handle multipart/form-data file uploads for book cover images
 *            • Save uploaded files to wwwroot/img/books/ using a GUID prefix
 *            • Delete orphaned image files when a book is updated or removed
 *
 * WHAT STUDENTS LEARN HERE (beyond CategoriesController basics):
 *  1. Eager loading (.Include)  — explicitly load a related entity in ONE query
 *                                 using a SQL JOIN, rather than issuing a separate
 *                                 SELECT for each book's category.
 *  2. SelectList + ViewBag      — how to pass a dropdown data source from the
 *                                 controller to the view for a <select> element.
 *  3. [Bind] without navigation — always exclude navigation properties (Category,
 *                                 CartItems, OrderDetails) from [Bind] to prevent
 *                                 over-posting attacks.
 *  4. Repopulate ViewBag        — if form validation fails, we must rebuild the
 *                                 SelectList and put it back in ViewBag, otherwise
 *                                 the dropdown disappears when the form re-renders.
 *  5. OrderBy in queries        — sort the book list and category dropdowns with
 *                                 LINQ OrderBy / ThenBy to improve usability.
 *  6. IFormFile file upload     — receive a multipart/form-data file from the
 *                                 browser, validate it, and persist it to disk.
 *  7. IWebHostEnvironment       — injected service that exposes WebRootPath,
 *                                 the absolute filesystem path to wwwroot/.
 *  8. GUID file names           — prefix uploaded files with a Guid to avoid
 *                                 collisions and prevent directory traversal attacks.
 *  9. File cleanup on edit/delete — delete the old image file from disk when a
 *                                 book's cover is replaced or the book is removed.
 * 10. AsNoTracking + projection — read just the existing Image column with no
 *                                 EF change tracking overhead before an edit POST.
 *
 * POST-SCAFFOLDING FIXES APPLIED (documented inline below):
 *  Step 1 → Added "using Microsoft.AspNetCore.Mvc.Rendering" for SelectList
 *  Step 2 → Added .Include(b => b.Category) to Index, Details, and Delete GET
 *  Step 3 → Added ViewBag.CategoryId SelectList to Create GET and Edit GET
 *  Step 4 → Removed navigation properties from [Bind] lists (Create & Edit POST)
 *  Step 5 → Repopulated ViewBag.CategoryId when ModelState is invalid (Create & Edit POST)
 *  Step 6 → Removed Image from [Bind]; added IFormFile? Image parameter for file upload
 *  Step 7 → Added IWebHostEnvironment injection for wwwroot path resolution
 *  Step 8 → Added UploadImage() and DeleteImage() private helpers
 *  Step 9 → Edit POST preserves existing image when no new file is uploaded
 *  Step 10→ Delete POST removes the associated image file from disk
 *  Step 11→ Edit POST accepts a deleteImage bool; when true (and no new file is
 *            uploaded) the existing cover is removed from disk and Image is set
 *            to null — triggered by the "Remove cover image" checkbox in Edit.cshtml
 * ============================================================================
 */

// FIX (Step 1): Microsoft.AspNetCore.Mvc.Rendering provides SelectList, which
// is needed to build the Category dropdown for the Create and Edit forms.
// The scaffolder does NOT add this using statement automatically.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetBookstore.Models;
using DotNetBookstore.Data;
using Microsoft.AspNetCore.Authorization;

// Only users in the "Customer" role may access this controller's actions.
// The Details action is decorated with [AllowAnonymous], so anyone can view
// book details without signing in.
// The "Customer" role is created in Program.cs and assigned to new users
// during registration.]
[Authorize(Roles = "Administrator")]
public class BooksController : Controller
{
    // Injected via constructor by the DI container (see Program.cs).
    private readonly ApplicationDbContext _context;

    // FIX (Step 7): IWebHostEnvironment exposes WebRootPath — the absolute
    // filesystem path to the wwwroot/ folder. Using it here is more reliable
    // than Directory.GetCurrentDirectory() because WebRootPath always resolves
    // to the actual web root regardless of the working directory at startup.
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BooksController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // ── GET: /Books ───────────────────────────────────────────────────────────
    // Lists all books, ordered by Author then Title, with their Category loaded.
    //
    // FIX (Step 2): Without .Include(b => b.Category), EF Core returns books
    // where book.Category is null. The view would then show an empty category
    // column or throw a NullReferenceException.
    //
    // .Include(b => b.Category) tells EF Core to perform a JOIN:
    //   SELECT b.*, c.* FROM Books b INNER JOIN Categories c ON b.CategoryId = c.CategoryId
    // OrderBy / ThenBy add ORDER BY Author, Title to the query for alphabetical listing.
    public async Task<IActionResult> Index()
    {
        return View(await _context.Books
            .Include(b => b.Category)         // eagerly load the related Category
            .OrderBy(b => b.Author)           // primary sort: alphabetical by author
            .ThenBy(b => b.Title)             // secondary sort: alphabetical by title
            .ToListAsync());
    }

    // ── GET: /Books/Details/5 ─────────────────────────────────────────────────
    // Shows the full details of a single book, including its category name.
    //
    // FIX (Step 2): Same .Include() fix — the Details view displays
    // book.Category.Name, so we must load the related Category or it will be null.
    [AllowAnonymous]  // anyone can view book details, even if not logged in
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books
            .Include(b => b.Category)    // JOIN with Categories table
            .FirstOrDefaultAsync(m => m.BookId == id);

        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    // ── GET: /Books/Create ────────────────────────────────────────────────────
    // Returns an empty form for adding a new book.
    //
    // FIX (Step 3): The scaffolded version had no SelectList — it generated a
    // plain text box for CategoryId (an integer FK field), which is poor UX.
    // We replace that with a <select> dropdown in the view, and this SelectList
    // feeds it with all available categories.
    //
    // new SelectList(source, valueField, displayField):
    //   source      → IQueryable from the Categories table
    //   valueField  → "CategoryId" — the integer stored in the Books.CategoryId FK
    //   displayField→ "Name"       — the human-readable string shown in the dropdown
    public IActionResult Create()
    {
        // Pass the SelectList to the view via ViewBag.
        // The key "CategoryId" matches the asp-items="ViewBag.CategoryId" attribute
        // in the Create.cshtml view's <select> element.
        ViewBag.CategoryId = new SelectList(
            _context.Categories.OrderBy(c => c.Name), // source: all categories, A-Z
            "CategoryId",                              // value stored when selected
            "Name");                                   // text shown to the user
        return View();
    }

    // ── POST: /Books/Create ───────────────────────────────────────────────────
    // Receives the form submission, validates it, and inserts the new book.
    //
    // FIX (Step 4): The scaffolded [Bind] list included navigation properties
    // (Category, CartItems, OrderDetails). Navigation properties must NEVER be
    // bound from raw POST data — doing so opens an over-posting vulnerability
    // where a malicious user could submit spoofed related-object data.
    // Solution: only bind the scalar, user-editable fields.
    //
    // FIX (Step 6): Image is excluded from [Bind] because the uploaded file
    // arrives as an IFormFile in the browser's multipart/form-data encoding —
    // it is NOT a simple string value that can be bound directly from the form.
    // The IFormFile? Image parameter receives the raw file stream separately.
    // Book.Image is a nullable string that stores ONLY the saved filename (GUID prefix
    // + original name), never a URL. The view reconstructs the full path as
    // ~/img/books/{filename} when rendering the <img> tag.
    //
    // FIX (Step 5): When validation fails, we return the form view — but the
    // ViewBag.CategoryId we set in the GET action is gone (ViewBag is per-request).
    // We must rebuild it here and include book.CategoryId as the 4th argument
    // so the dropdown pre-selects the category the user had already chosen.
    [HttpPost]
    [ValidateAntiForgeryToken]
    // Image is intentionally excluded from [Bind]; it is received as IFormFile below.
    public async Task<IActionResult> Create(
        [Bind("BookId,Author,Title,Price,MatureContent,CategoryId")] Book book,
        IFormFile? Image)
    {
        if (ModelState.IsValid)
        {
            // FIX (Step 8): Call UploadImage() to save the file to wwwroot/img/books/
            // and store the generated filename on the book entity.
            if (Image != null && Image.Length > 0)
            {
                // UploadImage returns the GUID-prefixed filename (e.g., "abc123-cover.jpg").
                // The full URL path ~/img/books/{filename} is built in the views.
                book.Image = UploadImage(Image);
            }
            else
            {
                // No file uploaded — Image stays null (the view will show the placeholder icon).
                book.Image = null;
            }

            _context.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // FIX (Step 5): Repopulate the category dropdown with the previously
        // selected value pre-selected, so the user doesn't lose their choice.
        ViewBag.CategoryId = new SelectList(
            _context.Categories,
            "CategoryId",
            "Name",
            book.CategoryId);  // 4th arg = currently selected value
        return View(book);
    }

    // ── GET: /Books/Edit/5 ────────────────────────────────────────────────────
    // Fetches the book and returns a pre-filled edit form.
    //
    // FIX (Step 3): Same SelectList setup as Create GET, with the addition of
    // book.CategoryId as the 4th argument — this pre-selects the book's current
    // category in the dropdown when the Edit form opens.
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // FindAsync is efficient for PK lookups — checks the EF identity cache first.
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        // Build the SelectList with the book's current CategoryId pre-selected
        // so the dropdown shows the correct category when the Edit form opens.
        ViewBag.CategoryId = new SelectList(
            _context.Categories.OrderBy(c => c.Name),
            "CategoryId",
            "Name",
            book.CategoryId);  // pre-select the book's existing category
        return View(book);
    }

    // ── POST: /Books/Edit/5 ───────────────────────────────────────────────────
    // Saves the updated book fields to the database.
    //
    // FIX (Step 4):  Same [Bind] fix as Create POST.
    // FIX (Step 6):  Image excluded from [Bind]; received as IFormFile? Image.
    // FIX (Step 9):  If no new file is uploaded, the existing image filename is
    //                read from the database (AsNoTracking to avoid tracking conflicts)
    //                and restored onto the book entity before saving. This prevents
    //                accidentally wiping the cover image when a user edits other fields.
    // FIX (Step 11): deleteImage bool is posted by the "Remove cover image" checkbox
    //                in Edit.cshtml. When true and no new file is selected, the old
    //                cover file is deleted from disk and Image is set to null.
    //                Priority: new file upload > delete > preserve existing.
    [HttpPost]
    [ValidateAntiForgeryToken]
    // Image is intentionally excluded from [Bind]; it is received as IFormFile below.
    // deleteImage arrives from the unchecked/checked checkbox; defaults to false when unchecked.
    public async Task<IActionResult> Edit(int? id,
        [Bind("BookId,Author,Title,Price,MatureContent,CategoryId")] Book book,
        IFormFile? Image,
        bool deleteImage = false)
    {
        // Route ID must match the hidden BookId field in the form.
        if (id != book.BookId)
        {
            return NotFound();
        }

        // FIX (Step 9 + 10): Load the existing image filename BEFORE the
        // ModelState check so we can:
        //   a) restore it if validation fails (view re-render still shows the image)
        //   b) delete the old file when a new image is uploaded
        // AsNoTracking() is critical here: we don't want EF to track this read
        // because we're about to call _context.Update(book) on the same entity ID.
        // If both were tracked, EF would throw an "already tracking an entity with
        // the same key" exception.
        var existingImage = await _context.Books
            .AsNoTracking()
            .Where(b => b.BookId == book.BookId)
            .Select(b => b.Image)
            .FirstOrDefaultAsync();

        if (ModelState.IsValid)
        {
            try
            {
                if (Image != null && Image.Length > 0)
                {
                    // Priority 1: A new cover file was uploaded — save it and delete the old one.
                    // This takes precedence even if the delete checkbox was also checked.
                    var newFileName = UploadImage(Image);

                    // FIX (Step 10): Remove the old image file from disk to avoid
                    // orphaned files accumulating in wwwroot/img/books/.
                    if (!string.IsNullOrEmpty(existingImage))
                    {
                        DeleteImage(existingImage);
                    }

                    book.Image = newFileName;
                }
                else if (deleteImage)
                {
                    // FIX (Step 11): Priority 2 — the "Remove cover image" checkbox was
                    // checked and no replacement file was uploaded. Delete the old file
                    // from disk and clear the Image column so the placeholder renders.
                    if (!string.IsNullOrEmpty(existingImage))
                    {
                        DeleteImage(existingImage);
                    }
                    book.Image = null;
                }
                else
                {
                    // Priority 3: No new file and delete not requested — preserve the
                    // existing filename so the cover is not lost when editing other fields.
                    book.Image = existingImage;
                }

                // Update() attaches the detached entity and marks all fields as modified.
                // SaveChangesAsync() generates: UPDATE Books SET ... WHERE BookId = @id
                _context.Update(book);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if the book still exists; if not, it was deleted by someone else.
                if (!BookExists(book.BookId))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Unknown concurrency problem — propagate to error handler
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // Validation failed — restore the existing image so the view re-renders
        // the current cover correctly without losing it on the round-trip.
        book.Image = existingImage;

        // FIX (Step 5): Repopulate the dropdown on validation failure.
        ViewBag.CategoryId = new SelectList(
            _context.Categories,
            "CategoryId",
            "Name",
            book.CategoryId);
        return View(book);
    }

    // ── GET: /Books/Delete/5 ─────────────────────────────────────────────────
    // Shows a confirmation page with the book's details before deletion.
    //
    // FIX (Step 2): .Include(b => b.Category) so the confirmation page can
    // show the category name alongside the book title, author, and price.
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books
            .Include(b => b.Category)  // load category so the view can display its name
            .FirstOrDefaultAsync(m => m.BookId == id);
        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    // ── POST: /Books/Delete/5 ────────────────────────────────────────────────
    // Permanently removes the book after the user confirms on the delete page.
    //
    // ActionName("Delete") allows both the GET and POST Delete actions to share
    // the same URL (/Books/Delete/5) while having different C# method names.
    //
    // FIX (Step 10): Delete the associated image file from wwwroot/img/books/
    // before removing the database record to avoid orphaned files on disk.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book != null)
        {
            // FIX (Step 10): Remove the image file from disk before deleting
            // the database row. If we deleted the row first and the file delete
            // failed, the file would be permanently orphaned with no way to find it.
            if (!string.IsNullOrEmpty(book.Image))
            {
                DeleteImage(book.Image);
            }

            _context.Books.Remove(book);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ── Private helper: UploadImage ───────────────────────────────────────────
    // Saves an uploaded cover image to wwwroot/img/books/ and returns the
    // GUID-prefixed filename that is stored in the Books.Image database column.
    //
    // WHY a GUID prefix?
    //   Two users could upload files with the same name (e.g., "cover.jpg").
    //   A GUID prefix (e.g., "a3f9c12b-...-cover.jpg") makes every stored filename
    //   unique across the entire application and prevents one upload from silently
    //   overwriting another. It also prevents directory traversal attacks where an
    //   attacker names a file "../../web.config" — the GUID prefix blocks that.
    //
    // WHY IWebHostEnvironment.WebRootPath instead of Directory.GetCurrentDirectory()?
    //   WebRootPath always resolves to the actual wwwroot/ directory regardless of
    //   the process working directory. GetCurrentDirectory() can differ depending on
    //   how the application is started (Visual Studio vs. dotnet run vs. IIS).
    private string UploadImage(IFormFile image)
    {
        // Guard: return empty if the file is null or has no content.
        if (image == null || image.Length == 0)
        {
            return string.Empty;
        }

        // Build the absolute path to wwwroot/img/books/ using WebRootPath.
        var imagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "books");

        // Create the directory if it does not exist yet (first run, or after clean checkout).
        if (!Directory.Exists(imagesPath))
        {
            Directory.CreateDirectory(imagesPath);
        }

        // Prepend a GUID to guarantee a unique filename on disk.
        // Format: "a3f9c12b-0000-0000-0000-000000000000-originalname.jpg"
        var fileName = Guid.NewGuid().ToString() + "-" + image.FileName;

        // Combine the directory path and the unique filename to get the full write path.
        var uploadPath = Path.Combine(imagesPath, fileName);

        // Open a FileStream to the destination path and copy the uploaded bytes into it.
        // FileMode.Create overwrites any file that somehow already exists at that path.
        using (var stream = new FileStream(uploadPath, FileMode.Create))
        {
            image.CopyTo(stream);
        }

        // Return only the filename (not the full path).
        // Views reconstruct the URL as: ~/img/books/{fileName}
        return fileName;
    }

    // ── Private helper: DeleteImage ───────────────────────────────────────────
    // Removes a previously-uploaded cover image file from wwwroot/img/books/.
    // Called when a book is deleted or its cover is replaced with a new upload.
    // Silently does nothing if fileName is null/empty or the file no longer exists.
    private void DeleteImage(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "books", fileName);

        // Only attempt deletion if the file actually exists — avoids an exception
        // if the file was already manually removed or never written to disk.
        // NOTE: Must qualify as System.IO.File because Controller inherits a File()
        // method from ControllerBase that would otherwise shadow the static File class.
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
    }

    // ── Private helper ────────────────────────────────────────────────────────
    // Checks if a book with the given ID exists. Used in the Edit POST action
    // to handle DbUpdateConcurrencyException gracefully.
    private bool BookExists(int? id)
    {
        return _context.Books.Any(e => e.BookId == id);
    }
}