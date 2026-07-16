// =============================================================================
// DotNet Bookstore — In-Class ASP.NET Core MVC Project
// Course: COMP2084 | Georgian College | Summer 2026
// Instructor: Dario Hesami
// =============================================================================
//
// FILE:    wwwroot/js/site.js
// PURPOSE: Global client-side JavaScript for the DotNet Bookstore application.
//          Loaded by _Layout.cshtml on every page (after jQuery and Bootstrap JS).
//
// CURRENT STATUS:
//   This file is intentionally minimal. The live image preview logic lives
//   directly in Books/Create.cshtml and Books/Edit.cshtml (inside their
//   @section Scripts blocks) because it is page-specific and should only
//   run on those pages.
//
//   Add GLOBAL JavaScript here — code that needs to run on EVERY page
//   (e.g., analytics, global keyboard shortcuts, universal toast timers).
//
// KEY CONCEPTS FOR STUDENTS:
//   1. wwwroot/         — the "web root" folder. Files here are served directly
//                         as static assets by the web server. The URL path
//                         maps to the file path: /js/site.js → this file.
//   2. Load order       — in _Layout.cshtml, scripts are loaded in this order:
//                           1. jQuery (~/lib/jquery/dist/jquery.min.js)
//                           2. Bootstrap JS (~/lib/bootstrap/dist/js/bootstrap.bundle.min.js)
//                           3. This file (~/js/site.js)
//                           4. @RenderSectionAsync("Scripts") — page-specific scripts
//                         Any code here can use jQuery ($) and Bootstrap (bootstrap.*).
//   3. DOMContentLoaded — wrapping code in a DOMContentLoaded listener ensures
//                         it runs AFTER the HTML is fully parsed, so getElementById
//                         and querySelector calls will find their elements.
//   4. Separation of concerns — keep page-specific JS in the view's @section Scripts.
//                         Put only truly shared, site-wide JS here.
//
// EXAMPLE: Auto-dismiss Bootstrap alerts after 5 seconds
// (Uncomment to enable — useful for toast notifications in _Layout.cshtml)
// =============================================================================

// document.addEventListener('DOMContentLoaded', function () {
//     /*
//      * Auto-dismiss all dismissible Bootstrap alerts after 5 000 ms (5 seconds).
//      * This improves UX for success/error toast messages — the user sees them
//      * briefly and they disappear without requiring a manual close click.
//      *
//      * bootstrap.Alert — the Bootstrap 5 JS API for alerts.
//      * querySelectorAll — finds ALL elements matching the CSS selector.
//      * setTimeout       — runs a callback after a delay (in milliseconds).
//      */
//     document.querySelectorAll('.alert-dismissible').forEach(function (alertEl) {
//         setTimeout(function () {
//             var bsAlert = bootstrap.Alert.getOrCreateInstance(alertEl);
//             bsAlert.close(); // triggers Bootstrap's fade-out animation
//         }, 5000); // 5 000 ms = 5 seconds
//     });
// });
