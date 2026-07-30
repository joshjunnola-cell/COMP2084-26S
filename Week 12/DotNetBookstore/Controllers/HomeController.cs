/*
 * ============================================================================
 * DotNet Bookstore — In-Class ASP.NET Core MVC Project
 * Course: COMP2084 | Georgian College | Summer 2026
 * Instructor: Dario Hesami
 * ============================================================================
 *
 * FILE:    Controllers/HomeController.cs
 * PURPOSE: Handles the application's "non-data" pages — the landing page,
 *          privacy policy, and the global error page. These actions do not
 *          query the database; they simply return pre-built views.
 *
 * KEY CONCEPTS FOR STUDENTS:
 *  1. MVC Controller     — a class that inherits from Controller and contains
 *                          action methods. The URL routing engine maps HTTP
 *                          requests to the correct controller + action based
 *                          on the URL pattern configured in Program.cs.
 *  2. Action method      — a public method inside a controller that handles
 *                          one specific HTTP request and returns an IActionResult.
 *  3. IActionResult      — the return type for all actions. View() is one kind;
 *                          others include RedirectToAction(), NotFound(),
 *                          Ok(), Json(), etc.
 *  4. return View()      — tells MVC to find and render the Razor view whose
 *                          path matches the controller name + action name:
 *                          HomeController.Index() → Views/Home/Index.cshtml
 *  5. [ResponseCache]    — an attribute that controls HTTP caching headers.
 *                          Duration=0 + NoStore=true tells browsers and proxies
 *                          NEVER to cache the Error page, so stale error pages
 *                          are never served from a browser cache.
 *  6. ErrorViewModel     — a ViewModel passed to the Error view so it can
 *                          display the request ID for bug reporting / logging.
 * ============================================================================
 */

using DotNetBookstore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DotNetBookstore.Controllers
{
    // HomeController does NOT need DI for a database context because it has no
    // data-driven actions. Compare this with BooksController which injects
    // ApplicationDbContext into its constructor.
    public class HomeController : Controller
    {
        // ── GET: / or /Home or /Home/Index ───────────────────────────────────
        // The default landing page of the application.
        // Route: matched by the default pattern in Program.cs → controller=Home, action=Index
        // Returns: Views/Home/Index.cshtml (the hero banner + feature cards)
        public IActionResult Index()
        {
            return View();
        }

        // ── GET: /Home/Privacy ────────────────────────────────────────────────
        // Returns the static privacy policy page.
        // This is a placeholder — in a real application you would fill in actual
        // legal text about data collection and user rights.
        public IActionResult Privacy()
        {
            return View();
        }

        // ── GET: /Home/Error ─────────────────────────────────────────────────
        // Rendered automatically when an unhandled exception occurs in production.
        // Configured in Program.cs: app.UseExceptionHandler("/Home/Error").
        //
        // [ResponseCache] — prevents caching of this page entirely:
        //   Duration = 0      → cache for 0 seconds (effectively no caching)
        //   NoStore = true    → do not store in any cache (browser or proxy)
        //   Location = None   → do not specify a cache location
        // This is critical: you don't want a user to see a cached error page
        // from a previous session — each error should be shown fresh.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Activity.Current?.Id captures the distributed trace ID (if any),
            // which can be matched against server logs to find the root cause.
            // If no trace is active, fall back to HttpContext.TraceIdentifier.
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
