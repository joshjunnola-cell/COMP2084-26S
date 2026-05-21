# DotNetBookstore

DotNetBookstore is a .NET 10 ASP.NET Core web application for a simple bookstore learning project. It uses MVC controllers and views for the main site experience, with Razor Pages provided through ASP.NET Core Identity for authentication-related pages.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Available Pages and Routes](#available-pages-and-routes)
- [Database](#database)
- [Development Notes](#development-notes)
- [License](#license)

## Overview

This repository contains a bookstore sample application built with ASP.NET Core. The current implementation focuses on:

- a home page,
- a categories listing page,
- a category browsing page, and
- built-in ASP.NET Core Identity infrastructure.

The project appears to be intended for learning and coursework use, including ASP.NET Core MVC concepts such as controllers, views, routing, dependency injection, Entity Framework Core, and Identity.

## Features

- ASP.NET Core web application targeting **.NET 10**
- MVC controller and view pattern for the main application flow
- Razor Pages support for Identity UI
- Entity Framework Core with SQL Server provider
- Default ASP.NET Core Identity integration
- Category listing and category browsing pages
- Static asset support for CSS, JavaScript, Bootstrap, and jQuery

## Tech Stack

- **Framework:** ASP.NET Core 10
- **Runtime Target:** .NET 10 (`net10.0`)
- **UI Pattern:** MVC with Razor Views, plus Razor Pages for Identity
- **Data Access:** Entity Framework Core 10
- **Database Provider:** SQL Server / LocalDB
- **Authentication:** ASP.NET Core Identity
- **Frontend Libraries:** Bootstrap, jQuery, jQuery Validation

## Project Structure

```text
DotNetBookstore/
├── README.md
└── DotNetBookstore/
	├── Areas/
	│   └── Identity/
	├── Controllers/
	│   ├── CategoriesController.cs
	│   └── HomeController.cs
	├── Data/
	│   ├── ApplicationDbContext.cs
	│   └── Migrations/
	├── Models/
	│   ├── Category.cs
	│   └── ErrorViewModel.cs
	├── Properties/
	│   └── launchSettings.json
	├── Views/
	│   ├── Categories/
	│   ├── Home/
	│   └── Shared/
	├── wwwroot/
	├── appsettings.json
	├── appsettings.Development.json
	├── DotNetBookstore.csproj
	└── Program.cs
```

## Getting Started

### Prerequisites

Install the following before running the app:

- .NET 10 SDK
- SQL Server LocalDB or SQL Server
- Visual Studio 2026 or another compatible .NET IDE/editor

### Clone the Repository

```bash
git clone https://github.com/Dario-Hesami/DotNetBookstore-S26.git
cd DotNetBookstore
```

### Restore Dependencies

```bash
dotnet restore DotNetBookstore/DotNetBookstore.csproj
```

### Apply the Database

If LocalDB is available and the default connection string is valid for your environment, update the database with:

```bash
dotnet ef database update --project DotNetBookstore/DotNetBookstore.csproj
```

If the Entity Framework CLI tool is not installed, you may need:

```bash
dotnet tool install --global dotnet-ef
```

### Run the Application

```bash
dotnet run --project DotNetBookstore/DotNetBookstore.csproj
```

By default, the development launch settings define these local URLs:

- `http://localhost:5170`
- `https://localhost:7296`

## Configuration

The main configuration file is:

- `DotNetBookstore/appsettings.json`

The default connection string uses LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-DotNetBookstore-066cf498-1be6-4125-920f-9291eae32e12;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

If you are using a different SQL Server instance, update `DefaultConnection` before running migrations or starting the app.

## Available Pages and Routes

The current application includes the following main routes:

- `/` - Home page
- `/Home/Privacy` - Privacy page
- `/Categories` - Category list
- `/Categories/Browse?category=CategoryName` - Browse a selected category
- `/Identity/...` - ASP.NET Core Identity pages

The default route is configured as:

```text
{controller=Home}/{action=Index}/{id?}
```

## Database

The app uses `ApplicationDbContext`, which currently derives from `IdentityDbContext`. This means the database is currently set up primarily for ASP.NET Core Identity tables.

The repository also includes Entity Framework Core migration files under:

- `DotNetBookstore/Data/Migrations/`

At the moment, the category list shown in the app is generated in memory inside `CategoriesController` rather than being loaded from a database table.

## Development Notes

- The application uses `AddControllersWithViews()` for MVC support.
- Identity is configured with `RequireConfirmedAccount = true`.
- Static assets are mapped through the ASP.NET Core asset pipeline.
- The project includes Bootstrap and jQuery in `wwwroot/lib`.
- The categories feature is currently a simple learning example and can be expanded into full CRUD functionality backed by Entity Framework Core.

## License

This repository includes a `LICENSE.txt` file. See that file for the project license terms.
