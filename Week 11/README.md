# DotNet Bookstore

A full-featured ASP.NET Core MVC web application for managing an online bookstore catalogue. Built as part of the **COMP2084** course at **Georgian College** (Summer 2026).

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?logo=bootstrap)](https://getbootstrap.com/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-brightgreen)](https://docs.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Azure-CC2927?logo=microsoftsqlserver)](https://azure.microsoft.com/en-us/products/azure-sql/)

---

## Live Repository

[https://github.com/Dario-Hesami/DotNetBookstore-S26](https://github.com/Dario-Hesami/DotNetBookstore-S26)

---

## Table of Contents

- [Overview](#overview)
- [Educational Comments](#educational-comments)
- [Technology Stack](#technology-stack)
- [Features](#features)
- [UI/UX Design](#uiux-design)
- [Data Models](#data-models)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Opening in VS Code](#opening-in-vs-code)
- [Database Setup](#database-setup)
- [Configuration](#configuration)
- [Available Routes](#available-routes)
- [Deploying to Azure Web App](#deploying-to-azure-web-app)
- [Documentation](#documentation)
- [UI Enhancement History](#ui-enhancement-history)

---

## Overview

DotNet Bookstore is an ASP.NET Core MVC CRUD application that manages a bookstore's catalogue of books and categories. It uses Entity Framework Core with SQL Server (Azure) for data persistence, ASP.NET Core Identity for authentication, and a fully custom Bootstrap 5 design system for a modern, responsive UI.

The project demonstrates real-world patterns including:

- One-to-many relational data (Category → Books)
- EF Core eager loading with `.Include()`
- Scaffolded CRUD controllers with post-scaffolding fixes
- ASP.NET Core Identity (registration, login, logout)
- Bootstrap 5 responsive layout with Bootstrap Icons
- CSS custom properties for a consistent design token system
- Reactive UI elements (live cover image preview via FileReader API, empty states, hover effects)
- **File upload** — cover images uploaded via `multipart/form-data`, saved to `wwwroot/img/books/` with GUID-prefixed filenames; old files deleted on replace or book removal

---

## Educational Comments

> **Students and learners — please read this section before diving into the code.**

Every source file in this repository has been annotated with two layers of purposeful, curriculum-aligned comments. They are not boilerplate — they are a structured learning resource written specifically for this course. Treat them as an integrated textbook that lives right next to the code it explains.

### What was added and why

| Layer | Where you find it | What it teaches |
| --- | --- | --- |
| **File header block** | Top of every `.cs`, `.cshtml`, `.css`, and `.js` file | States the file's purpose in plain English and lists the specific ASP.NET Core / C# concepts demonstrated inside |
| **Inline comments** | Alongside individual lines, blocks, and decisions throughout each file | Explains *why* the code is written the way it is — the reasoning, the trade-offs, and the common mistakes it avoids |

### Commented files and their key concepts

| File | Key concepts covered in the comments |
|---|---|
| `Program.cs` | `WebApplicationBuilder`, Dependency Injection, EF Core registration, Identity setup, middleware pipeline order, convention-based routing |
| `Data/ApplicationDbContext.cs` | `DbContext` vs `IdentityDbContext`, `DbSet<T>` as a table, constructor injection of `DbContextOptions`, migrations workflow, optional Fluent API |
| `Models/Category.cs` | POCO model, primary key by convention, `[Required]` / `[DisplayName]` data annotations, navigation properties |
| `Models/Book.cs` | Foreign key property, navigation property, `[ValidateNever]`, `null!` null-forgiving operator, `[Range]`, `[DisplayFormat]` |
| `Models/CartItem.cs` | Database-persisted cart pattern, price snapshot, `CustomerId` as identity key, FK + navigation property pair |
| `Models/Order.cs` | Order header / detail split pattern, `decimal` for currency, capturing shipping data at checkout time |
| `Models/OrderDetail.cs` | Junction/bridge table concept, two FKs in one model, price capture, `[ValidateNever]` on navigation properties |
| `Models/ErrorViewModel.cs` | ViewModel vs. entity, expression-bodied property (`=>`), nullable reference types |
| `Controllers/HomeController.cs` | MVC controller anatomy, action methods, `IActionResult`, `return View()`, `[ResponseCache]` |
| `Controllers/CategoriesController.cs` | Full CRUD HTTP mapping, `async`/`await`, `[HttpPost]`, `[ValidateAntiForgeryToken]`, `[Bind]`, `ModelState.IsValid`, `DbUpdateConcurrencyException`, PRG pattern |
| `Controllers/BooksController.cs` | All of the above **plus**: eager loading with `.Include()`, `SelectList` + `ViewBag`, repopulating dropdowns on validation failure, the 11 numbered post-scaffolding fixes; `IFormFile` file upload, `IWebHostEnvironment.WebRootPath`, GUID filename strategy, `UploadImage()` / `DeleteImage()` helpers, preserving existing image on edit, deleting orphaned files on delete, `bool deleteImage` parameter for explicit cover removal |
| `Views/Shared/_Layout.cshtml` | Master layout template, `@RenderBody()`, `@RenderSectionAsync`, TempData flash messages, active nav link detection, `asp-append-version` cache busting |
| `Views/Shared/_LoginPartial.cshtml` | Partial views, `@inject` in Razor, `SignInManager`, logout as a POST request (CSRF prevention) |
| `Views/Shared/_ValidationScriptsPartial.cshtml` | Client-side vs. server-side validation, jQuery Validate, unobtrusive validation bridge |
| `Views/Shared/Error.cshtml` | `@model` directive, ViewModel in a view, production vs. development error pages |
| `Views/_ViewImports.cshtml` | Global `@using` namespaces, `@addTagHelper`, Tag Helper overview |
| `Views/_ViewStart.cshtml` | `_ViewStart` implicit hook, setting the default layout, per-view overrides |
| `Views/Home/Index.cshtml` | Razor code blocks, `@*...*@` server-side comments, `asp-controller`/`asp-action`/`asp-route-id`, Bootstrap grid, responsive utilities; anonymous-type mock arrays (`new[] { new { … } }`), `@foreach` rendering, custom card classes for bestsellers and featured books |
| `Views/Books/Index.cshtml` | Strongly-typed `@model`, `.ToList()` to avoid double-enumeration, `@foreach`, conditional rendering, `asp-route-id`, empty state UX |
| `Views/Books/Create.cshtml` | `asp-action` on `<form>`, `enctype="multipart/form-data"` for file upload, `<input type="file">` with `accept`, `asp-for` on labels and text inputs, `asp-validation-for`, `asp-items` + `ViewBag` dropdown, FileReader API live preview IIFE, `@section Scripts` |
| `Views/Books/Edit.cshtml` | Hidden `BookId` field, `enctype="multipart/form-data"`, existing cover thumbnail with `~/img/books/` path, file input to replace image, **"Remove current cover image" checkbox** (`name="deleteImage" value="true"`), FileReader live preview for new selection, JS that dims thumbnail on checkbox check and auto-unchecks on new file selection, pre-populated inputs, category pre-selection |
| `Views/Books/Details.cshtml` | Read-only view, `@Html.DisplayFor`, `<dl>` definition list, null-conditional `?.`, two-column Bootstrap layout |
| `Views/Books/Delete.cshtml` | Two-step GET→POST delete, hidden PK field, danger styling conventions, Cancel as a safe link |
| `Views/Categories/*.cshtml` | All of the above patterns applied to a simpler, one-field entity — ideal for seeing the concepts in their most direct form |
| `wwwroot/css/site.css` | CSS custom properties (design tokens), Bootstrap override strategy, transitions, responsive `@media` queries, `object-fit: cover`, accessibility focus rings; new sections 18–20 cover bestseller amber cards, featured indigo cards with `backdrop-filter` glass badge, and the `.home-section-divider` gradient rule |
| `wwwroot/js/site.js` | `wwwroot` as the web root, script load order, `DOMContentLoaded`, separation of concerns, commented auto-dismiss example |

### How to get the most out of the comments

1. **Read the header block first.** Before studying any method or template, read the `KEY CONCEPTS` list at the top of the file. It tells you exactly what to watch for.
2. **Trace a full request.** Pick one action — for example, creating a book — and follow it from `BooksController.Create (GET)` → `Views/Books/Create.cshtml` → `BooksController.Create (POST)`. Read every inline comment along that path.
3. **Compare Create and Edit.** The `Create` and `Edit` views are nearly identical. The inline comments highlight the exact lines that differ and explain *why* (hidden PK field, pre-selected dropdown, image pre-load). This comparison is a fast way to understand model binding.
4. **Read the controller before the view.** The controller comment explains what data it prepares (`ViewBag`, `SelectList`, `.Include()`). Once you understand what arrives at the view, the view's markup makes much more sense.
5. **Don't skip the CSS.** `site.css` comments explain every design decision — from CSS custom properties to the book cover `object-fit` trick. If you plan to customise the UI, read Section 1 (Design Tokens) first.
6. **Ask "why", not just "what".** Most inline comments are written to answer *why* the code is written a particular way — a security reason (`[Bind]`, `[ValidateAntiForgeryToken]`), a convention (`null!`, `string.Empty`), or a fix to a scaffolding gap (`.Include()`, `SelectList`). Look for those explanations.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| Language | C# 14 |
| ORM | Entity Framework Core 10 |
| Database | SQL Server (Azure SQL Database / LocalDB) |
| Authentication | ASP.NET Core Identity |
| Frontend | Bootstrap 5, Bootstrap Icons 1.11.3 (CDN), jQuery |
| Custom CSS | `site.css` — full design system with CSS custom properties |
| IDE | Visual Studio Enterprise 2026 / VS Code + C# Dev Kit |

---

## Features

### Books Management

- **List** all books in a **responsive card grid** (1 → 2 → 3 → 4 columns) with cover art, category badge, Mature Content badge, author, title, and price
- **Create** a new book with author, title, cover image file upload (PNG/JPEG; live FileReader preview), price, mature content toggle, and category dropdown
- **Edit** an existing book — existing cover thumbnail shown; upload a new file to replace it, or check **"Remove current cover image"** to delete it (leave both empty to keep current); JS dims the thumbnail while the delete checkbox is active and auto-unchecks it if a new file is selected; category dropdown pre-selects the current value
- **View Details** — two-column card layout with full-height cover image on the left and metadata on the right
- **Delete** — danger-themed confirmation card with a cover thumbnail and full book summary; the associated image file is removed from disk on confirmation

### Categories Management

- **List** all categories in a **responsive card grid** (1 → 2 → 3 → 4 columns) with icon, name, and action buttons
- **Create**, **Edit**, **View Details**, and **Delete** categories with consistent card-wrapped forms and danger confirmation flow

### Authentication
- User registration and login via ASP.NET Core Identity
- Email confirmation required (`RequireConfirmedAccount = true`)
- Navbar displays logged-in username with manage and logout links

### UX Enhancements

- **Active nav link** highlighting — current controller auto-detected at render time
- **TempData toast area** in the layout — controllers can set `TempData["SuccessMessage"]` or `TempData["ErrorMessage"]` for dismissible Bootstrap alerts
- **Empty states** on index pages — icon, message, and CTA when no records exist
- **Live cover preview (FileReader API)** — on Create, when a file is selected in the picker the FileReader API reads the bytes locally and shows a preview before the form is submitted; on Edit, the existing cover thumbnail is shown server-side and a new preview appears only when a replacement file is chosen
- **Delete cover image** — a "Remove current cover image" checkbox in the Edit form lets users permanently delete the current cover; the controller deletes the file from disk and sets `Image = null`; JavaScript dims the thumbnail as feedback while the checkbox is active
- **Graceful image fallback** — gradient placeholder with a book icon is always rendered behind cover images; shows through on `onerror` with no JS required

---

## UI/UX Design

All views share a unified design language built on **Bootstrap 5** and a custom CSS design token system in `site.css`. Bootstrap Icons 1.11.3 is loaded via CDN.

### Design Tokens (`site.css`)

A set of CSS custom properties drives the entire visual theme:

```css
--brand-gradient        /* deep indigo → purple → indigo (navbar, footer) */
--hero-gradient         /* darker 3-stop indigo gradient (home hero) */
--brand-gradient-light  /* soft indigo→lavender (card covers, placeholders) */
--card-shadow           /* subtle multi-layer shadow */
--card-shadow-hover     /* lifted indigo-tinted shadow on hover */
--radius-card           /* .75rem — all cards and alerts */
--radius-btn            /* .5rem — all buttons and inputs */
--transition            /* 220ms ease — card and button transitions */
```

### Global Layout (`_Layout.cshtml`)

- **Navbar** — deep indigo gradient (`--brand-gradient`), amber brand badge icon, active link highlighted in gold, smooth background-fade on hover. Active state is detected server-side via `ViewContext.RouteData.Values["controller"]`.
- **Footer** — matching indigo gradient, responsive three-column layout (brand name / copyright / Privacy + GitHub links).
- **TempData alerts** — success and error banners rendered automatically between the navbar and main content area; dismissible via Bootstrap's `btn-close`.
- `min-vh-100` flex-column body keeps the footer pinned at the bottom on short pages.

### Home Page (`Home/Index.cshtml`)

- **Hero section** — full-width indigo/purple gradient card with an SVG dot-pattern texture overlay, large display heading with amber accent, lead text, and two CTA buttons (Browse Books / Categories).
- **Best Sellers section** — three amber/gold-themed cards with circular rank badges (#1, #2, #3) overlaid on the cover area, amber top-border accent, genre pill, title/author, price, and a "View Book" CTA. Mock data (Book IDs 1–3) replaces a real DB query; inline comments explain the production replacement path.
- **Featured Books section** — three indigo-themed cards with frosted-glass curator badges ("Editor's Choice", "Course Pick", "Staff Pick") pinned to the top-right of the cover, indigo left-border accent, and the same metadata layout. Mock data (Book IDs 4–6).
- **Section dividers** — `.home-section-divider` — a decorative 1 px rule with opacity fade at both edges, separating the three content sections.
- **Feature cards** — three equal-height navigation cards (Books, Categories, Account) with lift-on-hover effect, coloured icon badges, and action buttons; heading now includes a compass Bootstrap Icon.
- **Tech strip** — expanded to four items: Identity, EF Core, Bootstrap 5, and **File Upload** (GUID-based cover images), using a 2 × 2 grid on mobile and 4-column row on desktop.

### Books Views

| View | Design |
|---|---|
| `Index` | Responsive card grid; cover art with gradient-placeholder fallback behind image; category + Mature badges; price in green; icon action buttons in card footer |
| `Create` / `Edit` | Centred card form; `input-group` with Bootstrap Icons; small uppercase muted labels; `type="file"` cover image picker with FileReader live preview; Edit shows existing cover thumbnail + **"Remove current cover image" checkbox** (dims thumbnail via JS while active, auto-unchecks on new file selection); Mature Content toggle in a highlighted box; `divider-gradient` separator before action buttons |
| `Details` | Two-column card — full-height cover (or gradient placeholder) on left, metadata on right; category and rating badges; price in large green text |
| `Delete` | Red-bordered card with cover thumbnail, two-column summary; danger alert banner with triangle icon; "Delete Permanently" button |

### Categories Views

| View | Design |
|---|---|
| `Index` | Responsive card grid; tag icon in a rounded success-tinted badge; lift-on-hover; icon action buttons in card footer |
| `Create` / `Edit` | Centred card form; tag icon `input-group`; `autofocus` on Create; divider before buttons |
| `Details` | Gradient header band (indigo-subtle) with tag icon + category name; Edit / Delete / Back buttons |
| `Delete` | Danger header band + danger alert warning about book assignments; "Delete Permanently" button |

### Shared Components

- **Error page** — large danger octagon icon, `alert-secondary` for Request ID, yellow-tinted card for dev-mode info
- **Privacy page** — centred card with shield icon and divider
- **Empty states** — faint indigo icon, muted heading, small descriptor text, and a CTA button; used on both Books and Categories index pages

---

## Data Models

```
Category
├── CategoryId (PK)
└── Name (required)

Book
├── BookId (PK)
├── Author (required, max 100)
├── Title (required, max 200)
├── Image (nullable; stores GUID-prefixed filename saved to wwwroot/img/books/)
├── Price (required, 0.01–10000)
├── MatureContent (bool)
├── CategoryId (FK → Category)
└── Category (navigation property)

CartItem
├── CartItemId (PK)
├── Quantity (required, 1–1000)
├── Price (required)
├── BookId (FK → Book)
└── UserId (FK → IdentityUser)

Order
├── OrderId (PK)
├── OrderDate / OrderTotal
├── FirstName / LastName / Address
├── City / Province / PostalCode
├── Phone / Email
└── UserId (FK → IdentityUser)

OrderDetail
├── OrderDetailId (PK)
├── Quantity / Price
├── BookId (FK → Book)
└── OrderId (FK → Order)
```

---

## Project Structure

```
DotNetBookstore/
├── Controllers/
│   ├── HomeController.cs
│   ├── BooksController.cs          # Full CRUD with .Include() eager loading
│   └── CategoriesController.cs     # Full CRUD
├── Data/
│   ├── ApplicationDbContext.cs     # EF Core DbContext (Identity + 5 DbSets)
│   └── Migrations/                 # EF Core migration files
├── Models/
│   ├── Book.cs
│   ├── Category.cs
│   ├── CartItem.cs
│   ├── Order.cs
│   ├── OrderDetail.cs
│   └── ErrorViewModel.cs
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml          # Indigo gradient navbar + footer, active nav, toast area
│   │   ├── _LoginPartial.cshtml    # Auth nav links with Bootstrap Icons
│   │   └── Error.cshtml            # Styled error page with icon and dev-mode card
│   ├── Home/
│   │   ├── Index.cshtml            # Hero section, feature cards, tech strip
│   │   └── Privacy.cshtml          # Centred card with shield icon
│   ├── Books/
│   │   ├── Index.cshtml            # Responsive card grid with cover art and empty state
│   │   ├── Create.cshtml           # Card form with live cover preview
│   │   ├── Edit.cshtml             # Card form with live cover preview (pre-loaded)
│   │   ├── Details.cshtml          # Two-column cover + metadata card
│   │   └── Delete.cshtml           # Danger-bordered confirmation card with thumbnail
│   └── Categories/
│       ├── Index.cshtml            # Responsive card grid with empty state
│       ├── Create.cshtml           # Card form with autofocus
│       ├── Edit.cshtml             # Card form
│       ├── Details.cshtml          # Gradient header band card
│       └── Delete.cshtml           # Danger header band confirmation card
├── Areas/
│   └── Identity/                   # Scaffolded Identity pages
├── wwwroot/
│   ├── css/site.css                # Custom design system (tokens, components, utilities)
│   ├── js/site.js
│   ├── img/
│   │   └── books/                  # Uploaded book cover images (GUID-prefixed filenames)
│   └── lib/                        # Bootstrap 5, jQuery, jQuery Validation
├── Docs/
│   ├── Post-Scaffolding-Guide-OneToMany.md
│   └── Post-Model-Change-Migrations-Guide.md
├── appsettings.json                # Connection string (gitignored — do not commit credentials)
├── appsettings.Development.json
├── Program.cs
└── DotNetBookstore.csproj          # .NET 10, EF Core 10, Identity packages
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2026](https://visualstudio.microsoft.com/) (or VS Code with C# Dev Kit)
- SQL Server (local instance or Azure SQL Database)

### Clone the Repository

```bash
git clone https://github.com/Dario-Hesami/DotNetBookstore-S26.git
cd DotNetBookstore-S26
```

### Restore Dependencies

```bash
dotnet restore DotNetBookstore/DotNetBookstore.csproj
```

### Run the Application

```bash
dotnet run --project DotNetBookstore/DotNetBookstore.csproj
```

Or press **F5** in Visual Studio to launch with debugging. The default launch URLs are:

- `https://localhost:7296`
- `http://localhost:5170`

---

## Opening in VS Code

This repository includes a `.vscode/` configuration folder for a full VS Code development experience. These files are completely ignored by Visual Studio 2026 and do not affect that workflow in any way.

### VS Code Configuration Files

| File | Purpose |
|---|---|
| `.vscode/launch.json` | Two debug configs: **Launch (http)** on port 5170, **Launch (https)** on ports 7296/5170, plus Attach |
| `.vscode/tasks.json` | `build` (default Ctrl+Shift+B), `publish`, and `watch` tasks via `dotnet` CLI |
| `.vscode/settings.json` | Points to `DotNetBookstore.slnx`, format-on-save for C# and Razor, hides `bin/` and `obj/` from Explorer |
| `.vscode/extensions.json` | Workspace-recommended extensions (auto-prompted on first open) |

### Required Extensions

Accept the workspace prompt on first open, or install manually from the Extensions panel:

| Extension ID | Purpose |
|---|---|
| `ms-dotnettools.csharp` | C# language support — IntelliSense, diagnostics, go-to-definition |
| `ms-dotnettools.csdevkit` | C# Dev Kit — Solution Explorer, test runner, enhanced refactoring |
| `ms-dotnettools.vscode-dotnet-runtime` | .NET runtime installer used by the above extensions |

### Steps to Open and Run

1. **Open the folder** — `File → Open Folder` → select the `DotNetBookstore-S26/` root directory.
2. **Install extensions** — accept the prompt to install recommended extensions, then reload VS Code.
3. **Create `appsettings.json`** — this file is gitignored (keeps credentials out of source control). Create it at `DotNetBookstore/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DotNetBookstoreDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*"
   }
   ```

   > Alternatively, store the connection string in [User Secrets](#️-connection-string-security) instead of `appsettings.json`.

4. **Apply migrations** (first run only — skip if the database already exists from Visual Studio):

   ```bash
   dotnet ef database update --project DotNetBookstore/DotNetBookstore.csproj
   ```

5. **Run / Debug** — press `F5`, select **Launch (http)** or **Launch (https)** from the dropdown, and VS Code will build, start the app, and open the browser automatically.

   For hot-reload development, use the `watch` task instead (`Terminal → Run Task → watch`):

   ```bash
   dotnet watch run --project DotNetBookstore/DotNetBookstore.csproj
   ```

### Trust the HTTPS Dev Certificate (once per machine)

```bash
dotnet dev-certs https --trust
```

---

## Database Setup

The application uses **EF Core code-first** with two migrations:

| Migration | Description |
|---|---|
| `00000000000000_CreateIdentitySchema` | ASP.NET Core Identity tables |
| `20260529000439_CreateBookstoreTables` | Categories, Books, CartItems, Orders, OrderDetails |

Apply all migrations to a new database:

```bash
dotnet ef database update --project DotNetBookstore/DotNetBookstore.csproj
```

If the EF CLI tool is not installed:

```bash
dotnet tool install --global dotnet-ef
```

Add a new migration after model changes:

```bash
dotnet ef migrations add YourMigrationName --project DotNetBookstore/DotNetBookstore.csproj
dotnet ef database update --project DotNetBookstore/DotNetBookstore.csproj
```

See [`Docs/Post-Model-Change-Migrations-Guide.md`](DotNetBookstore/Docs/Post-Model-Change-Migrations-Guide.md) for detailed guidance.

---

## Configuration

### ⚠️ Connection String Security

**Never commit real credentials to source control.** Use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string" --project DotNetBookstore/DotNetBookstore.csproj
```

### Connection String Examples

**Local SQL Server (Windows Auth):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=dotnetbookstore;Trusted_Connection=True"
  }
}
```

**Azure SQL Database:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server.database.windows.net;Database=dotnetbookstore-azure;User Id=dba;Password=YOUR_PASSWORD;TrustServerCertificate=true"
  }
}
```

---

## Available Routes

| Route | Description |
|---|---|
| `/` | Home page (hero + best sellers + featured books + feature cards + tech strip) |
| `/Home/Privacy` | Privacy policy page |
| `/Books` | Books card grid |
| `/Books/Create` | Add a new book (with live cover preview) |
| `/Books/Edit/{id}` | Edit a book (cover preview pre-loaded) |
| `/Books/Details/{id}` | Two-column book detail view |
| `/Books/Delete/{id}` | Delete confirmation with thumbnail |
| `/Categories` | Categories card grid |
| `/Categories/Create` | Add a new category |
| `/Categories/Edit/{id}` | Edit a category |
| `/Categories/Details/{id}` | Category detail view |
| `/Categories/Delete/{id}` | Delete confirmation |
| `/Identity/Account/Register` | User registration |
| `/Identity/Account/Login` | User login |

Default route pattern: `{controller=Home}/{action=Index}/{id?}`

---

## Deploying to Azure Web App

This section walks you through deploying the DotNet Bookstore to **Azure App Service** (Azure Web Apps). Two main approaches are covered — Visual Studio 2026 and Azure Portal — and each approach includes both a **one-time publish** path and a **CI/CD pipeline** path. Database configuration and post-deployment verification are covered at the end.

> **Audience:** Students and learners deploying a course project to a live URL for the first time.  
> **Cost:** The Azure Free tier (F1) is sufficient for learning. You can also use Azure for Students credits.

---

### Prerequisites for Azure Deployment

Before you start, make sure you have:

| Requirement | Notes |
|---|---|
| **Azure account** | Sign up free at [azure.microsoft.com](https://azure.microsoft.com/free/). Students can use [Azure for Students](https://azure.microsoft.com/en-us/free/students/) for $100 free credit with no credit card. |
| **Azure SQL Database** | You will create this during the steps below. The app requires a SQL Server-compatible database in production (LocalDB is local-only). |
| **GitHub repository** | Required for the CI/CD path. The live repo is [github.com/Dario-Hesami/DotNetBookstore-S26](https://github.com/Dario-Hesami/DotNetBookstore-S26). |
| **Visual Studio 2026** | Required for Option 1. Install the **ASP.NET and web development** workload. |
| **.NET 10 SDK** | Already required for local development. |

---

### Step 0 — Create an Azure SQL Database (Required for Both Options)

The app uses SQL Server. Before deploying the web app you need a live database for it to connect to.

1. Sign in to the [Azure Portal](https://portal.azure.com).
2. Click **Create a resource** → search for **SQL Database** → click **Create**.
3. Fill in the **Basics** tab:
   - **Resource group:** Create new, e.g. `bookstore-rg`
   - **Database name:** e.g. `dotnetbookstore-db`
   - **Server:** Click **Create new**
     - Server name: any globally unique name, e.g. `bookstore-sqlserver-yourname`
     - Location: choose the region closest to your users
     - Authentication: **SQL authentication** — set an admin login and a strong password. Save these — you will need them for the connection string.
   - **Compute + storage:** Click *Configure database* → choose **Basic** (5 DTU, ~$5/month) or the **Serverless** tier for a free trial.
4. Go to **Networking** tab → set **Allow Azure services and resources to access this server** to **Yes**. This lets the Azure Web App reach the database.
5. Click **Review + create** → **Create**. Wait ~2 minutes for provisioning.
6. Once created, open the **SQL Database** resource → click **Connection strings** → copy the **ADO.NET** connection string. It looks like:

   ```text
   Server=tcp:bookstore-sqlserver-yourname.database.windows.net,1433;Initial Catalog=dotnetbookstore-db;Persist Security Info=False;User ID=<your-admin-login>;Password=<your-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```

   Replace `<your-admin-login>` and `<your-password>` with the values you set. **Keep this string safe — you will paste it into Azure App Service, never into source control.**

---

### Option 1 — Deploy Using Visual Studio 2026

Visual Studio's built-in Publish wizard is the fastest way to get your app live. It creates all Azure resources for you, generates a reusable publish profile, and can optionally wire up a GitHub Actions CI/CD pipeline.

#### 1A — One-Time Publish (Publish Wizard)

1. Open the solution in **Visual Studio 2026**.
2. In **Solution Explorer**, right-click the **DotNetBookstore** project → **Publish**.
3. The Publish wizard opens. Select **Azure** as the target → click **Next**.
4. Select **Azure App Service (Windows)** → click **Next**.

   > **Linux vs Windows:** Either works for .NET 10. Windows is the default and matches the development environment. Linux is slightly cheaper.

5. Sign in to your Azure account if prompted.
6. Under **App Service instances**, click the **+** (Create new) button:
   - **Name:** globally unique, e.g. `dotnetbookstore-yourname` — this becomes part of your URL (`https://dotnetbookstore-yourname.azurewebsites.net`)
   - **Subscription:** select your subscription
   - **Resource group:** select the `bookstore-rg` you created in Step 0 (or create a new one here)
   - **Hosting plan:** click **New** → give it a name, choose a region, set the **Size** to **Free (F1)** for learning or **Basic (B1)** for always-on
7. Click **Create** and wait ~1 minute for the resource to provision.
8. Back in the wizard, select the newly created App Service instance → click **Next**.
9. On the **API Management** step, click **Skip this step** → click **Finish**.
10. Visual Studio generates a publish profile and shows the **Publish summary** screen.
11. Before clicking Publish, configure the database connection string (see [Configure Connection Strings on Azure](#configure-connection-strings-and-app-settings-on-azure) below).
12. Once the connection string is set, click **Publish**. Visual Studio will:
    - Build the app in Release configuration
    - Package and upload it to Azure
    - Open the live URL in your browser when done

**To re-deploy after code changes:** just click **Publish** again on the same publish profile summary page. The profile is saved in `Properties/PublishProfiles/` inside the project.

---

#### 1B — CI/CD from Visual Studio (GitHub Actions)

Visual Studio can generate a GitHub Actions workflow for you so that every push to `main` automatically deploys to Azure.

1. Complete steps 1–10 above to create the App Service and publish profile.
2. On the **Publish summary** screen, find the **Continuous deployment** section and click **Edit** (or click the pencil icon next to the GitHub Actions entry).
3. Click **Configure** → sign in to GitHub and authorize Visual Studio to access your repository.
4. Select:
   - **Repository:** `Dario-Hesami/DotNetBookstore-S26`
   - **Branch:** `main`
5. Click **Finish**. Visual Studio:
   - Generates a workflow file at `.github/workflows/<profile-name>.yml`
   - Adds the publish profile as a GitHub secret (`AZUREAPPSERVICE_PUBLISHPROFILE_...`) to your repository automatically
   - Commits and pushes the workflow file
6. Go to your GitHub repository → **Actions** tab — you will see the workflow run. When it turns green your app is live.
7. From this point on, every `git push` to `main` triggers a new deployment automatically.

---

### Option 2 — Deploy Using the Azure Portal

Use this path if you prefer to stay in the browser, or when Visual Studio is not available.

#### 2A — Create the Azure Web App in the Portal

1. Sign in to the [Azure Portal](https://portal.azure.com).
2. Click **Create a resource** → search for **Web App** → click **Create**.
3. Fill in the **Basics** tab:

   | Field | Value |
   | --- | --- |
   | **Resource group** | `bookstore-rg` (or create new) |
   | **Name** | Globally unique, e.g. `dotnetbookstore-yourname` |
   | **Publish** | **Code** |
   | **Runtime stack** | **.NET 10 (STS)** |
   | **Operating System** | **Windows** (or Linux) |
   | **Region** | Region closest to your users |
   | **Pricing plan** | **Free F1** for learning, **Basic B1** for always-on |

4. Click **Review + create** → **Create**. Wait ~1 minute.

---

#### 2B — One-Time Deployment via Zip Deploy (Portal)

This method lets you deploy manually without Visual Studio by uploading a zip of the published output.

**Step 1 — Publish locally to a folder.**

In a terminal at the repo root:

```bash
dotnet publish DotNetBookstore/DotNetBookstore.csproj --configuration Release --output ./publish-output
```

**Step 2 — Zip the output.**

```powershell
# In PowerShell
Compress-Archive -Path .\publish-output\* -DestinationPath .\bookstore.zip
```

**Step 3 — Upload via Kudu (Advanced Tools).**

1. In the Azure Portal, open your **App Service** resource.
2. In the left menu, scroll to **Development Tools** → click **Advanced Tools** → **Go**.
3. In the Kudu interface, click **Tools** → **Zip Push Deploy**.
4. Drag and drop `bookstore.zip` onto the page, or use the file browser to upload it.
5. Kudu extracts the zip to `D:\home\site\wwwroot\` and restarts the app.
6. Navigate to `https://<your-app-name>.azurewebsites.net` to see the live app.

**Alternative — Azure CLI zip deploy:**

```bash
az webapp deploy --resource-group bookstore-rg --name dotnetbookstore-yourname --src-path bookstore.zip --type zip
```

---

#### 2C — Continuous Deployment with GitHub Actions (Portal)

This sets up a CI/CD pipeline directly from the Azure Portal using GitHub Actions.

**Step 1 — Enable GitHub Actions deployment.**

1. In the Azure Portal, open your **App Service** resource.
2. In the left menu, click **Deployment** → **Deployment Center**.
3. Under **Source**, select **GitHub**.
4. Click **Authorize** and sign in to GitHub to grant Azure access to your repositories.
5. Select:
   - **Organization:** `Dario-Hesami`
   - **Repository:** `DotNetBookstore-S26`
   - **Branch:** `main`
6. Under **Authentication type**, choose **Basic authentication** (simplest) or **User-assigned identity** (more secure).
7. Click **Save**.

**Step 2 — What Azure does automatically.**

Azure generates a workflow file and commits it to your repository at:

```text
.github/workflows/main_dotnetbookstore-yourname.yml
```

It also stores the publish profile as a GitHub repository secret so the workflow can authenticate without exposing credentials.

**Step 3 — Verify the pipeline.**

1. Go to your GitHub repository → **Actions** tab.
2. You will see a workflow run triggered by the commit Azure just made. Wait for it to turn green.
3. After that, every push to `main` automatically builds and deploys to Azure.

**Manually writing the workflow (optional — if you prefer full control).**

Create `.github/workflows/azure-deploy.yml` in your repository:

```yaml
name: Deploy to Azure Web App

on:
  push:
    branches:
      - main

env:
  AZURE_WEBAPP_NAME: dotnetbookstore-yourname   # Replace with your app name
  DOTNET_VERSION: '10.0.x'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Set up .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore DotNetBookstore/DotNetBookstore.csproj

      - name: Build
        run: dotnet build DotNetBookstore/DotNetBookstore.csproj --configuration Release --no-restore

      - name: Publish
        run: dotnet publish DotNetBookstore/DotNetBookstore.csproj --configuration Release --output ./publish-output --no-build

      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ env.AZURE_WEBAPP_NAME }}
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: ./publish-output
```

Then add the publish profile secret:

1. Azure Portal → App Service → **Overview** → **Download publish profile** → save the file.
2. GitHub repo → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**.
3. Name: `AZURE_WEBAPP_PUBLISH_PROFILE`, Value: paste the full contents of the downloaded `.PublishSettings` file.

---

### Configure Connection Strings and App Settings on Azure

The local `appsettings.json` is not deployed (it is `.gitignore`d). You must configure the connection string directly in Azure App Service — it is stored encrypted and injected as an environment variable at runtime, overriding anything in `appsettings.json`.

#### Set the Connection String

1. In the Azure Portal, open your **App Service**.
2. In the left menu, click **Settings** → **Environment variables**.
3. Click the **Connection strings** tab.
4. Click **+ Add**.
5. Fill in:
   - **Name:** `DefaultConnection`
   - **Value:** the Azure SQL connection string you copied in Step 0 (with your admin login and password filled in)
   - **Type:** `SQLAzure`
6. Click **Apply** → **Confirm**.

The app reads this at startup via:

```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Azure App Service automatically maps the `DefaultConnection` connection string setting to the `ConnectionStrings:DefaultConnection` configuration key that `GetConnectionString()` reads — no code changes required.

#### Set Additional App Settings

1. In **Settings** → **Environment variables** → **App settings** tab.
2. Click **+ Add** for each setting below:

   | Name | Value | Purpose |
   | --- | --- | --- |
   | `ASPNETCORE_ENVIRONMENT` | `Production` | Enables production error pages, disables developer exception page |

3. Click **Apply** → **Confirm**.

> **HTTPS Only:** In **Settings** → **Configuration** → **General settings**, enable **HTTPS Only** — this forces all HTTP traffic to redirect to HTTPS automatically.

---

### Apply Database Migrations on Azure

The Azure SQL Database is empty after creation. You must run EF Core migrations to create all the tables before the app will work.

#### Option A — Azure Cloud Shell (Recommended for Students)

1. In the Azure Portal, click the **Cloud Shell** icon (top toolbar, looks like `>_`).
2. Choose **Bash**.
3. Clone the repository into Cloud Shell:

   ```bash
   git clone https://github.com/Dario-Hesami/DotNetBookstore-S26.git
   cd DotNetBookstore-S26
   ```

4. Install the EF Core CLI tool:

   ```bash
   dotnet tool install --global dotnet-ef
   export PATH="$PATH:/home/$USER/.dotnet/tools"
   ```

5. Set the connection string as an environment variable (replace the placeholders):

   ```bash
   export ConnectionStrings__DefaultConnection="Server=tcp:bookstore-sqlserver-yourname.database.windows.net,1433;Initial Catalog=dotnetbookstore-db;Persist Security Info=False;User ID=your-admin;Password=your-password;Encrypt=True;"
   ```

6. Run migrations:

   ```bash
   dotnet ef database update --project DotNetBookstore/DotNetBookstore.csproj
   ```

7. You should see output ending with `Done.` — all tables are now created in Azure SQL.

#### Option B — Automatic Migrations in Program.cs

You can add a few lines to `Program.cs` to apply pending migrations automatically when the app starts. This is convenient but **use it only for development/learning** — in a real production app this approach can be dangerous if multiple instances start simultaneously.

```csharp
// Add this block in Program.cs AFTER builder.Build() and BEFORE app.Run()
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate(); // Applies any pending migrations on startup
}
```

After adding this, deploy the app. On first request the app will run migrations automatically.

---

### Verify the Deployed App (CRUD Testing)

After deployment and migrations, follow these steps to confirm the live app is fully functional:

#### 1 — Confirm the App Loads

- Navigate to `https://<your-app-name>.azurewebsites.net`
- The home page with the hero section, Best Sellers, and Featured Books should appear.
- If you see an error page, check: (a) connection string is correct, (b) migrations were applied, (c) `ASPNETCORE_ENVIRONMENT` is set to `Production`.

#### 2 — Register a User Account

- Click **Register** in the navbar → fill in an email and password → click **Register**.
- You should be redirected and logged in (the navbar shows your email).

> **Note:** The app has `RequireConfirmedAccount = true`. If you see a "confirm your email" message and have no email sender configured, temporarily set `RequireConfirmedAccount = false` in `Program.cs` for testing, then redeploy.

#### 3 — Test Categories CRUD

| Operation | Steps | Expected Result |
| --- | --- | --- |
| **Create** | Click **Categories** → **Create New** → enter a name → **Create** | New category appears in the grid |
| **Read** | Click **Details** on any category | Details card shows correctly |
| **Update** | Click **Edit** on a category → change the name → **Save** | Updated name appears in the grid |
| **Delete** | Click **Delete** on a category → **Delete Permanently** | Category removed from the grid |

#### 4 — Test Books CRUD

| Operation | Steps | Expected Result |
| --- | --- | --- |
| **Create** | Click **Books** → **Create New** → fill in author, title, price, select a category, optionally upload a cover image → **Create** | New book card appears in the grid with cover or placeholder |
| **Read** | Click **Details** on any book | Two-column card with cover image and metadata |
| **Update** | Click **Edit** → change a field, optionally replace or remove the cover image → **Save** | Changes reflected in the grid |
| **Delete** | Click **Delete** → **Delete Permanently** | Book removed; associated image file deleted from storage |

#### 5 — Verify File Upload (Cover Images)

1. Create a book and upload a PNG or JPEG cover image.
2. After saving, the book card should show the uploaded image.
3. To confirm the file physically exists on Azure, browse to it via Kudu:
   - Azure Portal → App Service → **Development Tools** → **Advanced Tools** → **Go**.
   - In the Kudu interface, click **Debug console → CMD**.
   - Navigate to `D:\home\site\wwwroot\img\books\` — you will see the GUID-prefixed file, e.g. `a3f7c2d1-..._yourimage.jpg`.

> **Important:** the file lives on Azure's persistent disk and survives app restarts, but it is deleted the next time you redeploy the app. See [Uploaded Images and Scale-Out](#uploaded-images-and-scale-out) for the full breakdown and storage capacity details.

---

### Troubleshooting: "Something Went Wrong" After Deployment

If you followed **Option 1 / 1A** and the app published without errors but you see this on a page like `/Books`:

```text
Something went wrong
An error occurred while processing your request.
Request ID: 00-...
```

the app started but crashed while handling that specific request. ASP.NET Core hides the real exception in Production mode to protect sensitive information. This section walks you through finding the actual error and applying the right fix.

---

#### Step 1 — See the Real Error (App Service Log Stream)

Find out *what* crashed before trying to fix it.

1. In the Azure Portal, open your **App Service**.
2. In the left menu, scroll to **Monitoring** → **Log stream**.
3. In a separate browser tab, navigate to the failing URL (e.g. `https://your-app.azurewebsites.net/Books`).
4. Switch back to the Log stream tab — the full exception and stack trace appear within a few seconds.

Read the error message. The most common messages and their fixes are in the sections below.

**Alternative — Kudu log files:**

1. Azure Portal → App Service → **Development Tools** → **Advanced Tools** → **Go**.
2. In the Kudu interface, navigate to **LogFiles** → **Application** to browse detailed error logs written to disk.

---

#### Step 2 — Temporarily Enable Detailed Error Pages (Optional)

If the Log stream output is hard to parse, you can make the live site show the full developer exception page — the same detailed view you get locally. **Do this only while diagnosing, then switch back immediately.**

1. Azure Portal → App Service → **Settings** → **Environment variables** → **App settings** tab.
2. Find `ASPNETCORE_ENVIRONMENT` and change its value from `Production` to `Development`.
3. Click **Apply** → **Confirm** and wait ~30 seconds for the app to restart.
4. Reload the failing URL — you will now see the full exception message, stack trace, and request details in the browser.
5. **Required:** After reading the error, set `ASPNETCORE_ENVIRONMENT` back to `Production` and click **Apply** → **Confirm**. Never leave a deployed app in Development mode.

---

#### Fix A — Connection String Missing or Incorrect (Most Common Cause)

**Symptom in logs:** `SqlException`, `Cannot open database`, `Login failed for user`, or `A network-related or instance-specific error occurred while establishing a connection to SQL Server`.

**Why it happens:** The local `appsettings.json` (which holds your LocalDB connection string) is listed in `.gitignore` and is never deployed. The Azure App Service has no connection string configured, so EF Core cannot reach the database and throws on the first query — which is why `/Books` crashes but the home page (which makes no DB calls) loads fine.

**Fix:**

1. Azure Portal → App Service → **Settings** → **Environment variables**.
2. Click the **Connection strings** tab.
3. Check whether an entry named `DefaultConnection` already exists:
   - **Missing:** click **+ Add** and fill in:
     - **Name:** `DefaultConnection`
     - **Value:** your Azure SQL connection string (copied from Step 0 in this guide)
     - **Type:** `SQLAzure`
   - **Exists but wrong:** click the pencil icon and verify every part of the string. Common mistakes:
     - Password contains a special character (`@`, `#`, `=`) that must be present verbatim — do not URL-encode it inside the connection string
     - Database name or server name has a typo
     - Server address is missing `.database.windows.net`
     - `<your-admin-login>` or `<your-password>` placeholders were never replaced
4. Click **Apply** → **Confirm** and wait ~30 seconds for the app to restart.
5. Reload `https://your-app.azurewebsites.net/Books` — the page should now load.

Your connection string should look exactly like this (with real values substituted):

```text
Server=tcp:bookstore-sqlserver-yourname.database.windows.net,1433;Initial Catalog=dotnetbookstore-db;Persist Security Info=False;User ID=your-admin-login;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

> **Where to find this string:** Azure Portal → your **SQL Database** resource → **Settings** → **Connection strings** → **ADO.NET** tab. Copy it and replace the `{your_password}` placeholder with your actual password.

---

#### Fix B — Migrations Not Applied (Tables Do Not Exist)

**Symptom in logs:** `Invalid object name 'Books'`, `Invalid object name 'Categories'`, `Invalid object name 'AspNetUsers'`, or similar — the connection is working but the database schema is empty.

**Why it happens:** The Azure SQL Database was created empty. EF Core does not create tables automatically unless you explicitly run migrations or call `Migrate()` in code.

**Quickest fix for students — add automatic migrations to `Program.cs`:**

1. Open `DotNetBookstore/Program.cs` in Visual Studio.
2. Find the line `var app = builder.Build();` and add the following block **immediately after it** (before `app.Run()`):

   ```csharp
   // Apply any pending EF Core migrations automatically on startup.
   // Suitable for learning projects; for production use a dedicated migration step instead.
   using (var scope = app.Services.CreateScope())
   {
       var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
       db.Database.Migrate();
   }
   ```

3. Save the file.
4. In Visual Studio, go to the **Publish** summary screen for your Azure profile and click **Publish** again to redeploy.
5. On the very first request after deployment the app will run all pending migrations and create every table in Azure SQL.
6. Navigate to `https://your-app.azurewebsites.net/Books` — it should now load the (empty) books list.

> **Alternative:** Run migrations manually from Azure Cloud Shell as described in the [Apply Database Migrations on Azure](#apply-database-migrations-on-azure) section above.

---

#### Fix C — Azure SQL Firewall Blocking the App Service

**Symptom in logs:** `Cannot open server '...' requested by the login. Client with IP address '...' is not allowed to access the server.`

**Fix:**

1. Azure Portal → your **SQL Server** resource (the logical server, not the database itself — look for the resource type *SQL server*).
2. In the left menu, click **Security** → **Networking**.
3. Under **Exceptions**, make sure **Allow Azure services and resources to access this server** is checked / set to **Yes**.
4. Click **Save**.

This firewall exception must be enabled for the Azure App Service to reach Azure SQL. Without it every connection attempt is blocked regardless of whether the connection string is correct.

---

#### Quick Diagnosis Checklist

Work through these in order until the app loads:

| # | Check | Where to verify |
| --- | --- | --- |
| 1 | Connection string is set in Azure App Service | Portal → App Service → Settings → Environment variables → **Connection strings** tab |
| 2 | Connection string name is exactly `DefaultConnection` (case-sensitive) | Same location — verify the **Name** field character-by-character |
| 3 | Connection string value has the correct server, database name, username, and password | Edit the entry and compare to Portal → SQL Database → Settings → Connection strings → ADO.NET |
| 4 | Azure SQL firewall allows Azure services | Portal → SQL Server → Security → Networking → **Allow Azure services and resources to access this server** = Yes |
| 5 | Migrations have been applied (tables exist) | Add `db.Database.Migrate()` to `Program.cs` and redeploy, **or** run `dotnet ef database update` from Azure Cloud Shell |
| 6 | App Service has restarted after config changes | Portal → App Service → **Overview** → **Restart** button — always restart after changing environment variables |

---

### Azure Deployment: Key Considerations

#### Cold Start on Free Tier

The **Free (F1)** tier unloads apps after 20 minutes of inactivity. The next request can take 10–30 seconds while the app reloads (this is called a **cold start**). For a course project this is usually acceptable.  
To eliminate cold starts, upgrade to **Basic (B1)** or higher and enable **Always On** under **Settings** → **Configuration** → **General settings**.

#### Uploaded Images and Scale-Out

When a user uploads a cover image through the live site, the file is written to `wwwroot/img/books/` at runtime. On Azure App Service (Windows hosting) this resolves to:

```text
D:\home\site\wwwroot\img\books\<guid-prefixed-filename>
```

The `D:\home\` volume is backed by **Azure's persistent networked storage (Azure Files)** — it is not a temporary RAM disk or OS temp folder. Files survive instance recycling and app restarts.

**However, redeployment wipes the folder.** This is the key practical limitation:

| Event | Uploaded images survive? |
| --- | --- |
| App restart / Azure recycles the process | **Yes** — `D:\home\` persists across restarts |
| **Redeploy via Visual Studio Publish** | **No** — Web Deploy's default behaviour replaces `wwwroot`, deleting all runtime-uploaded files |
| **Redeploy via GitHub Actions / Zip Deploy** | **No** — zip deploy replaces `wwwroot` entirely |
| Scale out to 2+ instances | **No** — images uploaded to one instance are invisible to requests served by another |

**How to browse uploaded files on Azure:**

1. Azure Portal → App Service → **Development Tools** → **Advanced Tools** → **Go**.
2. In the Kudu interface, click **Debug console → CMD** (or **PowerShell**).
3. Navigate to `D:\home\site\wwwroot\img\books\` — all uploaded cover images are listed here.

**Storage capacity on the Free (F1) tier:**

The entire `D:\home\` volume is capped at **1 GB**, shared across all apps in the App Service Plan. Your deployed app binaries occupy roughly 30–50 MB, leaving ~950 MB for runtime-uploaded files. At 100 KB–2 MB per cover image that is 500–9,000 images — plenty for a course project. Storage size is not the practical constraint; **redeployment is**.

| Tier | `D:\home\` storage quota |
| --- | --- |
| Free (F1) | 1 GB |
| Shared (D1) | 1 GB |
| Basic (B1) | 10 GB |
| Standard (S1) | 50 GB |
| Premium (P1v3) | 250 GB |

**Production-grade solution — Azure Blob Storage:**

For a real application, replace the local file-save logic in `BooksController.UploadImage()` with an upload to an **Azure Blob Storage** container, and store the blob URL in the `Image` column instead of a GUID filename. Uploaded files then live entirely outside the deployment path — they survive every redeploy, are visible to all scale-out instances, and storage is practically unlimited. This is outside the scope of the course but is the correct architectural next step.

#### Connection String Security

- **Never commit real connection strings to source control.** Use Azure App Service environment variables (as described above) or [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/) for production secrets.
- The local `appsettings.json` and `appsettings.Development.json` files are listed in `.gitignore` and should never be pushed.

#### Firewall Rules for Azure SQL

If you want to connect to the Azure SQL Database from your local machine (e.g., to run `dotnet ef database update` locally against the live database), you need to allow your IP:

1. Azure Portal → SQL Server resource → **Security** → **Networking**.
2. Under **Firewall rules**, click **Add your client IPv4 address**.
3. Click **Save**.

#### Summary of Azure Resources Created

| Resource | Name example | Purpose |
| --- | --- | --- |
| Resource Group | `bookstore-rg` | Logical container for all resources |
| App Service Plan | `bookstore-plan` | Defines compute tier and region |
| App Service (Web App) | `dotnetbookstore-yourname` | Hosts the ASP.NET Core MVC app |
| SQL Server | `bookstore-sqlserver-yourname` | Logical SQL Server instance |
| SQL Database | `dotnetbookstore-db` | Stores all app data |

---

## Documentation

The `Docs/` folder contains detailed developer guides:

| Document | Description |
|---|---|
| [`Post-Scaffolding-Guide-OneToMany.md`](DotNetBookstore/Docs/Post-Scaffolding-Guide-OneToMany.md) | Step-by-step fixes for scaffolded CRUD after adding one-to-many relationships — FK dropdowns, eager loading, Bind list trimming, dropdown repopulation |
| [`Post-Model-Change-Migrations-Guide.md`](DotNetBookstore/Docs/Post-Model-Change-Migrations-Guide.md) | Guide for safely adding EF Core migrations after model changes |

---

## UI Enhancement History

| Phase | Changes |
|---|---|
| Initial scaffold | Basic scaffolded CRUD views with default Bootstrap styling |
| Post-scaffolding fixes | Fixed FK dropdowns (`CategoryId` → `<select>`), added `.Include()` eager loading, replaced raw FK integers with human-readable category names, removed reverse navigation collection columns |
| Bootstrap 5 UI pass | Full overhaul of all 14 views: dark navbar with Bootstrap Icons 1.11.3 (CDN), hero home page with feature cards, hover tables with image thumbnails and colour badges, card-wrapped forms with `input-group` icons, `form-switch` for boolean fields, danger confirmation cards, styled error page, sticky dark footer |
| **Modern Design System** | Complete redesign of all 15 views and `site.css`: indigo/purple brand gradient replacing plain dark; CSS custom property token system (`--brand-gradient`, `--card-shadow`, `--radius-card`, etc.); Books and Categories index pages converted from tables to responsive card grids; live cover image preview (JS) in Create/Edit forms; layered gradient placeholder + real image on all cover slots with silent `onerror` fallback; two-column Details view (cover + metadata); active nav link via server-side controller detection; TempData toast area in layout; empty states with icon + CTA; page-header component with left accent border; `divider-gradient` separator; lift-on-hover transitions on all interactive cards and buttons |
| **File Upload** | Cover image upload via `multipart/form-data` `<input type="file">` replacing the previous URL text field. Files saved to `wwwroot/img/books/` with GUID-prefixed names to prevent collisions. `IWebHostEnvironment` injected into `BooksController` for `WebRootPath` resolution. `UploadImage()` helper writes the file; `DeleteImage()` helper removes orphaned files on cover replace or book delete. Edit form shows the existing thumbnail and preserves it when no new file is chosen. FileReader API drives the in-browser live preview on Create and Edit without a round-trip. `app.UseStaticFiles()` added to `Program.cs` so uploaded runtime files are served from `wwwroot/`. |
| **Delete Cover Image + Home Page Enhancements** | **Edit form:** "Remove current cover image" checkbox (`name="deleteImage" value="true"`) added to Edit.cshtml. Controller (Step 11 fix) accepts `bool deleteImage = false`; when true and no replacement file is uploaded, the old cover is deleted from disk and `Image` set to `null`. JS feedback: thumbnail dims (`opacity:0.3` + `grayscale(100%)`) when the checkbox is checked; auto-unchecks and restores the thumbnail when the user selects a new file (upload wins). **Home page:** two new sections added — *Best Sellers* (amber/gold theme, rank-badge overlay, `Book IDs 1–3` mock data) and *Featured Books* (indigo theme, glass-morphism curator badge, `Book IDs 4–6` mock data). Inline comments guide students on replacing mock data with a real DB query. `site.css` extended with sections 18–20: `.bestseller-card`, `.bestseller-rank-badge`, `.featured-card`, `.featured-label-badge`, `.home-section-divider`. Tech strip expanded from 3 to 4 icons (added File Upload). |

---

Built with love - Georgian College · COMP2084 Summer 2026
