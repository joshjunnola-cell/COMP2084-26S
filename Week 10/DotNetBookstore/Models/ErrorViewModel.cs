/*
 * ============================================================================
 * DotNet Bookstore — In-Class ASP.NET Core MVC Project
 * Course: COMP2084 | Georgian College | Summer 2026
 * Instructor: Dario Hesami
 * ============================================================================
 *
 * FILE:    Models/ErrorViewModel.cs
 * PURPOSE: A small ViewModel used exclusively by the Error view
 *          (Views/Shared/Error.cshtml) to pass error information from the
 *          HomeController.Error() action to the error page template.
 *
 * KEY CONCEPTS FOR STUDENTS:
 *  1. ViewModel vs. Entity   — an "entity" (like Book, Category) maps to a
 *                              database table. A "ViewModel" is a class built
 *                              specifically for a view — it may combine data
 *                              from multiple entities or add UI-only properties.
 *                              ErrorViewModel is a ViewModel: it has no EF
 *                              DbSet and never gets saved to the database.
 *  2. Expression-bodied property — ShowRequestId uses "=>" instead of a
 *                              traditional get { return ... } block. It is
 *                              equivalent but more concise, and is evaluated
 *                              fresh every time the property is read.
 *  3. Nullable string (?)    — RequestId can be null if no Activity trace is
 *                              currently active (e.g., in simpler error scenarios).
 *                              The "?" marks it as intentionally nullable.
 * ============================================================================
 */

namespace DotNetBookstore.Models
{
    public class ErrorViewModel
    {
        // The unique identifier of the HTTP request that caused the error.
        // Populated from Activity.Current?.Id (distributed tracing) or
        // HttpContext.TraceIdentifier as a fallback. Useful when reporting
        // bugs — the request ID lets the developer find the exact log entry.
        public string? RequestId { get; set; }

        // True only when RequestId has a non-empty value.
        // The Error view uses this to decide whether to show the Request ID panel.
        // Expression-bodied: this is identical to writing:
        //   get { return !string.IsNullOrEmpty(RequestId); }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
