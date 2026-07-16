# Post-Model-Change Migrations Guide
## Modifying Models After Creating the Database in ASP.NET Core + EF Core

**Project example used throughout this guide:** `DotNetBookstore` — specifically, changing the `Book` model after the initial database was already created and adding the `ChangeBookModel` migration.

---

## Table of Contents

1. [How EF Core Migrations Work — The Big Picture](#1-how-ef-core-migrations-work--the-big-picture)
2. [What `__EFMigrationsHistory` Is and Why It Matters](#2-what-__efmigrationshistory-is-and-why-it-matters)
3. [The Correct Workflow for Changing a Model](#3-the-correct-workflow-for-changing-a-model)
4. [Step 1 — Change Your Model Class](#4-step-1--change-your-model-class)
5. [Step 2 — Create a New Migration](#5-step-2--create-a-new-migration)
6. [Step 3 — Review the Generated Migration File](#6-step-3--review-the-generated-migration-file)
7. [Step 4 — Apply the Migration with `Update-Database`](#7-step-4--apply-the-migration-with-update-database)
8. [Understanding the Error: `There is already an object named 'X' in the database`](#8-understanding-the-error-there-is-already-an-object-named-x-in-the-database)
9. [Fix Option A — Drop and Recreate (Recommended for Development)](#9-fix-option-a--drop-and-recreate-recommended-for-development)
10. [Fix Option B — Repair the Migration History (Preserve Existing Data)](#10-fix-option-b--repair-the-migration-history-preserve-existing-data)
11. [Understanding the Decimal Precision Warnings](#11-understanding-the-decimal-precision-warnings)
12. [Fix for Decimal Precision Warnings](#12-fix-for-decimal-precision-warnings)
13. [Quick Troubleshooting Reference](#13-quick-troubleshooting-reference)
14. [Final Checklist](#14-final-checklist)

---

## 1. How EF Core Migrations Work — The Big Picture

When you use **Entity Framework Core**, your C# model classes (like `Book.cs`, `Category.cs`) are the **single source of truth** for your database schema. EF Core compares your current model to what the database already has and generates SQL commands to bring the database in line with the model.

This process is managed through **migrations** — C# files in the `Data/Migrations/` folder that represent a single, ordered, named change to the database schema.

Think of migrations as a **version-controlled changelog** for your database:

| Migration file | What it does to the database |
|---|---|
| `00000000000000_CreateIdentitySchema.cs` | Creates the ASP.NET Core Identity tables (`AspNetUsers`, etc.) |
| `20260529000439_CreateBookstoreTables.cs` | Creates `Categories`, `Books`, `CartItems`, `Orders`, `OrderDetails` |
| `20260604180143_ChangeBookModel.cs` | Alters `Books.Author` length, adds `Books.Newfield` column |

Each migration has two methods:
- `Up()` — applies the change (runs when you call `Update-Database`)
- `Down()` — reverses the change (runs when you roll back with `Update-Database -Migration <PreviousName>`)

EF Core applies migrations **in order**, from oldest to newest, and it knows which ones have already been applied by reading a special tracking table in the database called `__EFMigrationsHistory`.

---

## 2. What `__EFMigrationsHistory` Is and Why It Matters

`__EFMigrationsHistory` is a small table that EF Core creates automatically in your SQL Server database the very first time you run `Update-Database`. It is EF Core's own internal record-keeping system.

### Its structure

```sql
CREATE TABLE [__EFMigrationsHistory] (
	[MigrationId]     nvarchar(150) NOT NULL,   -- name of the migration file
	[ProductVersion]  nvarchar(32)  NOT NULL,   -- EF Core version that applied it
	CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);
```

### What a healthy history looks like

After a fresh `Update-Database` on this project, the table should contain:

| MigrationId | ProductVersion |
|---|---|
| `00000000000000_CreateIdentitySchema` | `10.0.x` |
| `20260529000439_CreateBookstoreTables` | `10.0.x` |
| `20260604180143_ChangeBookModel` | `10.0.x` |

### How EF Core uses this table every time you run `Update-Database`

1. EF Core reads `__EFMigrationsHistory` to find out which migrations are already recorded.
2. EF Core compares that list to all the migration files in your `Data/Migrations/` folder.
3. Any migration file **not yet in the history table** is considered **pending** and will be applied.
4. EF Core runs the `Up()` methods of all pending migrations in chronological order.
5. After each successful migration, EF Core inserts a new row into `__EFMigrationsHistory`.

> **Key rule:** EF Core trusts `__EFMigrationsHistory` completely. If a migration is recorded there, EF assumes the database already has those schema changes and will **never try to apply it again** — even if the tables were actually deleted afterwards. Conversely, if a migration is **not** in the history table, EF will always try to run it — even if the tables it creates already exist in the database.

---

## 3. The Correct Workflow for Changing a Model

Whenever you need to change a model class (add a property, rename a column, change a max length, add a new class/table), you must follow this three-step workflow **every time**:

```
1.  Edit your model class (.cs file)
		↓
2.  Add-Migration  <DescriptiveName>     ← creates a new migration file
		↓
3.  Update-Database                      ← applies the pending migration to SQL Server
```

**Never** modify the database structure directly in SQL Server Management Studio while EF Core is managing the schema. If you do, the migration history and the actual database schema go out of sync, which is exactly what causes the error this guide addresses.

---

## 4. Step 1 — Change Your Model Class

Open the model class you want to change and make your edits. In this project's example, `Book.cs` was modified in two ways:

### What changed in `Book.cs`

**Change 1 — Increase the max length of `Author`**

```csharp
// BEFORE
[MaxLength(100)]
public string Author { get; set; } = string.Empty;

// AFTER
[MaxLength(150)]    ← increased from 100 to 150 characters
public string Author { get; set; } = string.Empty;
```

**Change 2 — Add a new property `Newfield`**

```csharp
// AFTER — new property added at the bottom of the class
public string Newfield { get; set; } = string.Empty;
```

> **Important:** After editing the model, **do not** run `Update-Database` yet. You must create a migration first so EF knows what SQL to generate.

---

## 5. Step 2 — Create a New Migration

In Visual Studio, open **Tools → NuGet Package Manager → Package Manager Console** and run:

```
Add-Migration ChangeBookModel
```

Replace `ChangeBookModel` with a short, descriptive name that describes **what changed** — for example:
- `AddNewfieldToBooks`
- `IncreaseAuthorLength`
- `AddPublisherTable`

### What happens when you run `Add-Migration`

1. EF Core reads your current model classes.
2. EF Core reads the most recent `Designer.cs` snapshot file in `Data/Migrations/` (which records what the model looked like at the previous migration).
3. EF Core **diffs** the two and generates a new C# migration file representing only the delta — the new changes.
4. Two new files appear in `Data/Migrations/`:
   - `20260604180143_ChangeBookModel.cs` — the migration logic (`Up()` and `Down()`)
   - `20260604180143_ChangeBookModel.Designer.cs` — a snapshot used by the next `Add-Migration`

> **Never edit migration files by hand** unless you are experienced with EF Core migrations. Editing them incorrectly can corrupt your migration history.

---

## 6. Step 3 — Review the Generated Migration File

Always open and read the generated migration file before applying it. Confirm that it describes exactly what you intended to change — nothing more, nothing less.

### Example: `20260604180143_ChangeBookModel.cs`

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
	migrationBuilder.AlterColumn<string>(
		name: "Author",
		table: "Books",
		type: "nvarchar(150)",    ← correctly increased from 100 → 150
		maxLength: 150,
		nullable: false,
		oldClrType: typeof(string),
		oldType: "nvarchar(100)",
		oldMaxLength: 100);

	migrationBuilder.AddColumn<string>(
		name: "Newfield",         ← correctly adding the new column
		table: "Books",
		type: "nvarchar(max)",
		nullable: false,
		defaultValue: "");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
	migrationBuilder.DropColumn(
		name: "Newfield",         ← Down() correctly reverses the AddColumn
		table: "Books");

	migrationBuilder.AlterColumn<string>(
		name: "Author",
		table: "Books",
		type: "nvarchar(100)",    ← Down() correctly reverses the length back to 100
		maxLength: 100,
		...);
}
```

This looks correct — `Up()` adds `Newfield` and widens `Author`, and `Down()` reverses both.

---

## 7. Step 4 — Apply the Migration with `Update-Database`

In **Package Manager Console** run:

```
Update-Database
```

### What a successful run looks like

```
Build started...
Build succeeded.
Applying migration '20260604180143_ChangeBookModel'.
Done.
```

EF Core:
1. Connects to the database specified in `appsettings.json` → `ConnectionStrings:DefaultConnection`
2. Reads `__EFMigrationsHistory`
3. Finds that `ChangeBookModel` is not yet recorded
4. Runs its `Up()` method — alters the `Author` column and adds `Newfield`
5. Inserts a row for `ChangeBookModel` into `__EFMigrationsHistory`

If instead you see errors, read the next section.

---

## 8. Understanding the Error: `There is already an object named 'X' in the database`

### The full error message

```
Microsoft.Data.SqlClient.SqlException (0x80131904):
There is already an object named 'Categories' in the database.
Error Number:2714,State:6,Class:16
```

### Why this error happens

This error means EF Core tried to run `CREATE TABLE [Categories]` — but that table already exists in the database. EF should never try to create a table that already exists, so something must be wrong with the migration history.

The general cause is always the same: **the `__EFMigrationsHistory` table does not match the actual state of the database.** However, this mismatch can happen in two distinct ways — and identifying which one you have determines the correct fix.

---

### Cause 1 — History table is completely empty

```
What the database actually contains:         What __EFMigrationsHistory contains:
─────────────────────────────────────        ──────────────────────────────────────
✅ AspNetUsers (and other Identity tables)   (empty — no rows at all)
✅ Categories
✅ Books
✅ CartItems
✅ Orders
✅ OrderDetails
```

Because `__EFMigrationsHistory` is **empty**, EF Core believes **no migrations have ever been applied**. So it tries to start from scratch:

```
Step 1: Apply 00000000000000_CreateIdentitySchema  ← tries to CREATE Identity tables
Step 2: Apply 20260529000439_CreateBookstoreTables ← tries to CREATE TABLE Categories ← 💥 CRASH
```

#### How does the history table end up empty?

| Scenario | What happened |
|---|---|
| **Database was dropped and recreated manually** (via SSMS or a SQL script) without restoring `__EFMigrationsHistory` | The tables exist, but EF's tracking record was lost |
| **A backup/restore** was applied that did not include `__EFMigrationsHistory` | Same result |
| **`__EFMigrationsHistory` was accidentally deleted or truncated** | EF has no memory of previous runs |
| **The connection string was changed** to point to a different database that already has tables but has never been managed by EF | That database has no history table at all |
| **The database was created from a SQL script** (exported from another environment) rather than through EF migrations | Scripts typically don't include `__EFMigrationsHistory` |

**How to confirm:** Run `SELECT * FROM [__EFMigrationsHistory]` in a SQL query window. If it returns **0 rows**, this is your cause.

---

### Cause 2 — History table has rows, but the MigrationIds do not match the files on disk

This is a subtler and equally common problem. The history table is **not** empty — it has rows — but the `MigrationId` values stored in it no longer correspond to the migration files that actually exist in the project's `Data/Migrations/` folder.

**Example — what the history table contains:**

| MigrationId | ProductVersion |
|---|---|
| `00000000000000_CreateIdentitySchema` | `10.0.8` |
| `20260528175808_CreateBookstoreTables` | `10.0.8` ← timestamp `175808` |

**Example — what migration files exist on disk:**

```
Data/Migrations/
  00000000000000_CreateIdentitySchema.cs         ← matches history ✅
  20260529000439_CreateBookstoreTables.cs        ← timestamp 000439 — does NOT match history ❌
  20260604180143_ChangeBookModel.cs              ← not in history at all ❌
```

EF Core matches by **exact `MigrationId` string**. The history row `20260528175808_CreateBookstoreTables` does not match any file — it is a stale, orphaned record from a previous version of the migration. Meanwhile, the current file `20260529000439_CreateBookstoreTables` is not in the history, so EF treats it as pending and tries to run its `Up()` method — which tries to `CREATE TABLE [Categories]` on a database that already has it. Same crash, different cause.

```
EF Core's view of the world:
──────────────────────────────────────────────────────────────────────────
Migration file on disk                    Status EF assigns
──────────────────────────────────────────────────────────────────────────
00000000000000_CreateIdentitySchema     → found in history  → SKIP ✅
20260529000439_CreateBookstoreTables    → NOT in history    → PENDING ❌ → runs Up() → 💥 CRASH
20260604180143_ChangeBookModel          → NOT in history    → PENDING ❌
──────────────────────────────────────────────────────────────────────────
Stale orphan in history (no matching file):
20260528175808_CreateBookstoreTables    → EF ignores it (no file to run)
```

#### How does a MigrationId mismatch happen?

| Scenario | What happened |
|---|---|
| **The migration was deleted and recreated** with `Remove-Migration` + `Add-Migration` after the database had already been set up | `Add-Migration` generates a new timestamp, but the database still holds the old timestamp from the first run |
| **Working across branches or team members** where one developer ran `Add-Migration` at a different time than another | The timestamps in the filenames diverge between the code version and the database version |
| **The project was cloned or copied** from a different machine/repository where the original migration had a different timestamp | The new codebase has different filenames than what the database was originally built from |
| **A migration file was renamed** manually in the filesystem | The filename and history no longer agree |

**How to confirm:** Run `SELECT * FROM [__EFMigrationsHistory]` and compare each `MigrationId` value to the actual filenames in `Data/Migrations/`. If any stored ID does not exactly match a filename (minus `.cs`), this is your cause.

---

## 9. Fix Option A — Drop and Recreate (Recommended for Development)

This is the cleanest, simplest fix when you are in a development environment and **do not need to keep any existing data**.

### In Package Manager Console, run these two commands in order:

```
Drop-Database
```

```
Update-Database
```

### What each command does

**`Drop-Database`**
- Connects to the database named in `appsettings.json`
- Deletes the entire database from SQL Server — all tables, all rows, all history
- EF Core will ask you to confirm: type `Y` and press Enter

**`Update-Database`**
- Creates a brand new, empty database
- Applies all pending migrations in order:
  1. `00000000000000_CreateIdentitySchema` → creates Identity tables
  2. `20260529000439_CreateBookstoreTables` → creates `Categories`, `Books`, etc.
  3. `20260604180143_ChangeBookModel` → widens `Author`, adds `Newfield`
- Inserts a row for each migration into `__EFMigrationsHistory`
- The history is now complete and accurate

### Why this is the right choice for development

In a development or student project, the database typically contains only test data that can be recreated. A clean drop-and-recreate is fast, guaranteed to work, and leaves the database in a perfectly consistent state with zero manual SQL needed.

> **When NOT to use this option:** If your database contains production data or data that took significant effort to seed and cannot be recreated, use Option B instead.

---

## 10. Fix Option B — Repair the Migration History (Preserve Existing Data)

Use this approach when you need to **keep the existing data** in the database and only want EF Core to apply the one new migration (`ChangeBookModel`) that is actually pending.

First, **identify which sub-case you have** by running this in a SQL query window:

```sql
SELECT * FROM [__EFMigrationsHistory];
```

- If it returns **0 rows** → follow **Option B1 — Empty History** below.
- If it returns rows but some IDs **do not match** any filename in `Data/Migrations/` → follow **Option B2 — MigrationId Mismatch** below.

---

### Opening a query window

In Visual Studio: **View → SQL Server Object Explorer** → expand your server → expand `temp-w4` → right-click → **New Query**.

Or use SQL Server Management Studio (SSMS): connect to `DESKTOP-TIVMN8F`, open a query window, and set the target database to `temp-w4`.

---

### Option B1 — Empty History

Apply when `__EFMigrationsHistory` has **no rows at all**.

**Step 1 — Insert all previously-applied migrations**

This tells EF Core that these two migrations were already applied before the history was lost:

```sql
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES
	('00000000000000_CreateIdentitySchema',  '10.0.8'),
	('20260529000439_CreateBookstoreTables', '10.0.8');
```

> **Note on the version number:** Use the EF Core version your project targets (visible in the `.csproj` NuGet package reference). EF Core does not strictly validate this value, but keeping it accurate is good practice.

**Step 2 — Verify**

```sql
SELECT * FROM [__EFMigrationsHistory];
```

You should see exactly two rows.

**Step 3 — Run `Update-Database`**

```
Update-Database
```

EF Core will skip the two migrations already in the history and apply only the pending one:

```
Applying migration '20260604180143_ChangeBookModel'.
Done.
```

---

### Option B2 — MigrationId Mismatch

Apply when `__EFMigrationsHistory` **has rows, but one or more IDs do not match any file** currently in `Data/Migrations/`.

**Concrete example from this project:**

| History contains | Files on disk |
|---|---|
| `20260528175808_CreateBookstoreTables` ← stale, wrong timestamp | `20260529000439_CreateBookstoreTables.cs` ← correct, current file |

The fix is to **delete the stale row and replace it with the correct ID** that matches the actual file on disk.

**Step 1 — Delete the mismatched row**

```sql
DELETE FROM [dbo].[__EFMigrationsHistory]
WHERE [MigrationId] = '20260528175808_CreateBookstoreTables';
```

> Replace `20260528175808_CreateBookstoreTables` with whatever stale ID you see in your history that has no matching file.

**Step 2 — Insert the correct row**

```sql
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES ('20260529000439_CreateBookstoreTables', '10.0.8');
```

> The `MigrationId` value must be the **exact filename** of the migration file in `Data/Migrations/`, minus the `.cs` extension.

**Step 3 — Verify the history is now correct**

```sql
SELECT * FROM [__EFMigrationsHistory];
```

Expected result — every row should correspond to a file that exists in `Data/Migrations/`:

| MigrationId | ProductVersion |
|---|---|
| `00000000000000_CreateIdentitySchema` | `10.0.8` |
| `20260529000439_CreateBookstoreTables` | `10.0.8` |

**Step 4 — Run `Update-Database`**

```
Update-Database
```

EF Core will now see only `20260604180143_ChangeBookModel` as pending and apply just that migration:

```
Applying migration '20260604180143_ChangeBookModel'.
Done.
```

EF will:
- Run `ALTER TABLE [Books] ALTER COLUMN [Author] nvarchar(150) NOT NULL`
- Run `ALTER TABLE [Books] ADD [Newfield] nvarchar(max) NOT NULL DEFAULT N''`
- Insert a row for `ChangeBookModel` into `__EFMigrationsHistory`

Your existing data is untouched.

---

### After either Option B fix — confirming the history is complete

```sql
SELECT * FROM [__EFMigrationsHistory];
```

Expected final state:

| MigrationId | ProductVersion |
|---|---|
| `00000000000000_CreateIdentitySchema` | `10.0.8` |
| `20260529000439_CreateBookstoreTables` | `10.0.8` |
| `20260604180143_ChangeBookModel` | `10.0.8` |

---

## 11. Understanding the Decimal Precision Warnings

During `Update-Database`, you may also see these warning messages — one for each `decimal` property in the project:

```
Microsoft.EntityFrameworkCore.Model.Validation[30000]
	  No store type was specified for the decimal property 'Price' on entity type 'Book'.
	  This will cause values to be silently truncated if they do not fit in the default
	  precision and scale. Explicitly specify the SQL server column type that can
	  accommodate all the values in 'OnModelCreating' using 'HasColumnType', specify
	  precision and scale using 'HasPrecision', or configure a value converter using
	  'HasConversion'.
```

### Why these warnings appear

EF Core sees a `decimal` property in your model but does not know what SQL Server precision and scale to use for the column. SQL Server's default `decimal` type is `decimal(18,0)` — which means **zero decimal places**. A price like `29.99` would be silently stored as `30` without any error.

The warnings come from these four properties:

| Entity | Property | Issue |
|---|---|---|
| `Book` | `Price` | no explicit SQL type |
| `CartItem` | `Price` | no explicit SQL type |
| `Order` | `OrderTotal` | no explicit SQL type |
| `OrderDetail` | `Price` | no explicit SQL type |

### Are these warnings blocking `Update-Database`?

**No.** These are warnings, not errors. `Update-Database` still runs and completes successfully when these warnings are present. However, the warnings indicate a potential data precision problem that should be fixed to avoid silent data loss.

> **Note:** In this project the migration files already use `decimal(18,2)` in the generated SQL (because the `Add-Migration` was run when EF inferred a reasonable default). However EF Core still logs the warning during runtime model validation even when the migration SQL is correct, because the warning is based on the C# model class, not the migration file. The fix is to add explicit precision annotations to the model classes themselves.

---

## 12. Fix for Decimal Precision Warnings

There are two ways to fix the warnings. Choose **one**:

---

### Option 1 — Data Annotation on the model property (recommended for students)

Add `[Column(TypeName = "decimal(18,2)")]` to each `decimal` property. This requires adding `using System.ComponentModel.DataAnnotations.Schema;` at the top of the model file.

#### `Book.cs`

```csharp
using System.ComponentModel.DataAnnotations.Schema;

// BEFORE
public decimal Price { get; set; }

// AFTER
[Column(TypeName = "decimal(18,2)")]
public decimal Price { get; set; }
```

#### `CartItem.cs`

```csharp
using System.ComponentModel.DataAnnotations.Schema;

// BEFORE
public decimal Price { get; set; }

// AFTER
[Column(TypeName = "decimal(18,2)")]
public decimal Price { get; set; }
```

#### `Order.cs`

```csharp
using System.ComponentModel.DataAnnotations.Schema;

// BEFORE
public decimal OrderTotal { get; set; }

// AFTER
[Column(TypeName = "decimal(18,2)")]
public decimal OrderTotal { get; set; }
```

#### `OrderDetail.cs`

```csharp
using System.ComponentModel.DataAnnotations.Schema;

// BEFORE
public decimal Price { get; set; }

// AFTER
[Column(TypeName = "decimal(18,2)")]
public decimal Price { get; set; }
```

After editing the model files, run `Add-Migration FixDecimalPrecision` and then `Update-Database`. The migration generated will likely contain no SQL changes (because the database columns are already `decimal(18,2)` from the original migration), but the warnings will disappear because EF Core's model validator can now see the explicit type.

---

### Option 2 — Fluent API in `OnModelCreating` (more advanced)

In `ApplicationDbContext.cs`, add `HasPrecision()` calls inside `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
	base.OnModelCreating(builder);

	builder.Entity<Book>()
		.Property(b => b.Price)
		.HasPrecision(18, 2);

	builder.Entity<CartItem>()
		.Property(c => c.Price)
		.HasPrecision(18, 2);

	builder.Entity<Order>()
		.Property(o => o.OrderTotal)
		.HasPrecision(18, 2);

	builder.Entity<OrderDetail>()
		.Property(od => od.Price)
		.HasPrecision(18, 2);
}
```

This achieves the same result as Option 1 but keeps all schema configuration centralized in `DbContext` rather than scattered across model files.

> For student projects, **Option 1 (Data Annotation)** is simpler and keeps all information about a property in one place (the model class itself).

---

## 13. Quick Troubleshooting Reference

| Symptom | Root cause | Fix |
|---|---|---|
| `There is already an object named 'X' in the database` — history table is empty | `__EFMigrationsHistory` has no rows; EF replays all migrations from scratch | Use Fix Option A (Drop-Database + Update-Database) or Fix Option B1 (INSERT all previously-applied IDs into `__EFMigrationsHistory`) |
| `There is already an object named 'X' in the database` — history table has rows but same error occurs | A `MigrationId` in `__EFMigrationsHistory` does not match any filename in `Data/Migrations/`; EF treats the current file as pending and tries to recreate tables | Use Fix Option A or Fix Option B2: DELETE the stale mismatched row, INSERT the correct ID that matches the actual filename, then run `Update-Database` |
| `Update-Database` succeeds but then app crashes with column not found | Model was changed without creating a migration | Run `Add-Migration <Name>` then `Update-Database` |
| `Add-Migration` generates an empty migration (`Up()` has no code) | The model change was already captured in a previous migration or the model class was not saved before running `Add-Migration` | Save all files, verify the model change, then re-run `Add-Migration` |
| Decimal precision warnings every time `Update-Database` runs | `decimal` properties have no explicit SQL type annotation | Add `[Column(TypeName = "decimal(18,2)")]` to each `decimal` property or use `HasPrecision()` in `OnModelCreating` |
| `Update-Database` says `No migrations were applied` | All migration files are already in `__EFMigrationsHistory` | Nothing to do — the database is already up to date |
| `Update-Database` applies a migration that was already run, causing duplicate column errors | A row was manually deleted from `__EFMigrationsHistory` | Re-insert the missing row(s) into `__EFMigrationsHistory` and do not run `Update-Database` again until they are there |
| Cannot connect to SQL Server — `Update-Database` fails immediately | Wrong server name or connection string in `appsettings.json` | Verify `Server=` matches your actual SQL Server instance name and check `Integrated Security=True` or your credentials |
| `Drop-Database` removes the wrong database | `appsettings.json` `DefaultConnection` points to a shared or production database | Always verify your connection string before running `Drop-Database` |
| `Add-Migration` error: `No DbContext was found` | Package Manager Console default project is not set to the project containing `ApplicationDbContext` | In PM Console, change the **Default project** dropdown to `DotNetBookstore` and retry |

---

## 14. Final Checklist

Use this checklist every time you change a model class and need to update the database.

### Before making model changes

- [ ] Know which model class(es) you are changing and what the change is
- [ ] Confirm `appsettings.json` → `DefaultConnection` points to your intended **development** database (not production)

### Making the change

- [ ] Edit the model class (e.g., add a property, change `[MaxLength]`, add a new class)
- [ ] Save all files before running any EF Core commands

### Creating the migration

- [ ] In Package Manager Console, confirm the **Default project** dropdown shows the correct project (`DotNetBookstore`)
- [ ] Run `Add-Migration <DescriptiveName>` with a meaningful name describing what changed
- [ ] Open the generated migration file and verify `Up()` contains exactly the expected SQL changes
- [ ] If `Up()` is empty, the model change was not detected — save files and retry

### Applying the migration

- [ ] Run `Update-Database`
- [ ] Confirm the console shows `Applying migration '<Name>'` and `Done.`
- [ ] If you see `There is already an object named 'X'`, follow **Fix Option A** or **Fix Option B** from this guide

### If you used Fix Option A (Drop and Recreate)

- [ ] Ran `Drop-Database` and confirmed with `Y`
- [ ] Ran `Update-Database` — all three (or more) migrations applied cleanly
- [ ] Verified the app starts and connects to the new database correctly
- [ ] Re-seeded any required test data

### If you used Fix Option B (History Repair — preserve data)

- [ ] Ran `SELECT * FROM [__EFMigrationsHistory]` to identify whether the history is empty (B1) or has a mismatched ID (B2)
- [ ] **If B1 (empty):** Inserted the correct `MigrationId` rows for all previously applied migrations
- [ ] **If B2 (mismatch):** Deleted the stale row whose ID has no matching file on disk, then inserted the correct ID that matches the actual filename in `Data/Migrations/`
- [ ] Confirmed the history with `SELECT * FROM [__EFMigrationsHistory]` — every row matches a file on disk
- [ ] Ran `Update-Database` — only the one new pending migration was applied
- [ ] Verified existing data is still present in the affected tables

### Cleaning up decimal warnings (optional but recommended)

- [ ] Added `[Column(TypeName = "decimal(18,2)")]` to all `decimal` properties in `Book.cs`, `CartItem.cs`, `Order.cs`, `OrderDetail.cs`
- [ ] Ran `Add-Migration FixDecimalPrecision`
- [ ] Ran `Update-Database`
- [ ] Confirmed precision warnings no longer appear in the Package Manager Console output

### Post-change verification

- [ ] Project builds with no errors
- [ ] App runs and the modified table/columns are accessible
- [ ] `SELECT * FROM [__EFMigrationsHistory]` in SQL Server shows one row per migration file in `Data/Migrations/`
- [ ] The number of rows in `__EFMigrationsHistory` equals the number of non-`Designer.cs`, non-`Snapshot.cs` migration files in `Data/Migrations/`

---

## Appendix — How to Check Migration State at Any Time

### See all migration files on disk

In **Package Manager Console**:

```
Get-Migration
```

This lists every migration EF Core knows about, whether or not it has been applied.

### See which migrations are pending (not yet applied to the database)

```
Get-Migration | Where-Object { $_.Applied -eq $false }
```

### Directly query the migration history in the database

In SQL Server (SSMS or VS SQL query window):

```sql
SELECT [MigrationId], [ProductVersion]
FROM [__EFMigrationsHistory]
ORDER BY [MigrationId];
```

### Roll back the last migration (undo it from the database)

```
Update-Database -Migration <NameOfTheMigrationBeforeTheOneYouWantToRemove>
```

For example, to undo `ChangeBookModel` and go back to the state after `CreateBookstoreTables`:

```
Update-Database -Migration 20260529000439_CreateBookstoreTables
```

This runs the `Down()` method of `ChangeBookModel`, removing `Newfield` and reverting `Author` back to `nvarchar(100)`.

After rolling back, you can also remove the migration file entirely with:

```
Remove-Migration
```

> Only run `Remove-Migration` **after** rolling back — never delete a migration file that has already been applied to a database.

---

*Keep this document as the reference guide for safely making model changes and diagnosing migration/database state mismatches in any EF Core project.*
