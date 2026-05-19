# InventoryPro — Inventory Management System

A full-stack inventory management web application built with **ASP.NET Core 7**, **Blazor Server**, and **SQL Server**. It provides a clean UI for managing products, product variants, and stock levels through both a Blazor front-end and a REST API with Swagger documentation.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Data Models](#data-models)
- [API Reference](#api-reference)
- [Blazor Pages](#blazor-pages)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Database Setup](#database-setup)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [Dependencies](#dependencies)

---

## Features

- **Dashboard** — At-a-glance stats: total products, active products, and total stock units, plus a recent products table.
- **Product Management** — Create products with a name, HSN code, and dynamically added variants (e.g. Size → S, M, L).
- **Product Listing** — Paginated product list showing code, HSN code, variant count, stock level, and active status.
- **Stock In (Purchase)** — Add stock to any product; records a `PURCHASE` transaction.
- **Stock Out (Sale)** — Remove stock from any product with an insufficient-stock guard; records a `SALE` transaction.
- **REST API** — Full JSON API for all product and stock operations, accessible via Swagger UI in development.
- **CORS** — Configured to allow requests from any origin, supporting decoupled front-end scenarios.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 7 (`.NET 7`) |
| UI | Blazor Server |
| API | ASP.NET Core Web API (Controllers) |
| ORM | Entity Framework Core 7 |
| Database | Microsoft SQL Server (via `Microsoft.Data.SqlClient`) |
| API Docs | Swashbuckle / Swagger |
| Logging | Serilog (ASP.NET Core integration) |
| Object Mapping | AutoMapper 12 |
| CSS | Bootstrap 5 + custom `site.css` |

---

## Project Structure

```
InventorySystem/
├── Controllers/
│   └── ProductController.cs       # REST API endpoints for products & stock
├── Data/
│   └── ApplicationDbContext.cs    # EF Core DbContext
├── DTOs/
│   ├── ProductCreateDto.cs        # Input DTO for product creation (+ VariantDto)
│   ├── StockDto.cs                # Stock-related DTO
│   └── StockTransactionDto.cs     # Transaction DTO
├── Migrations/
│   └── 20260515105944_InitialCreate.cs   # Initial DB schema migration
├── Models/
│   ├── Product.cs                 # Core product entity
│   ├── ProductVariant.cs          # Variant (e.g. "Size") entity
│   ├── VariantOption.cs           # Option value (e.g. "M") entity
│   ├── StockTransaction.cs        # Audit record for stock movements
│   └── ErrorViewModel.cs          # MVC error model
├── Pages/                         # Blazor Server pages
│   ├── Index.razor                # Dashboard
│   ├── ProductList.razor          # Paginated product table
│   ├── CreateProduct.razor        # Product creation form
│   ├── AddStock.razor             # Add stock (purchase) form
│   ├── RemoveStock.razor          # Remove stock (sale) form
│   └── _Host.cshtml               # Blazor host page
├── Services/
│   ├── IProductService.cs         # Service interface
│   └── ProductService.cs          # EF Core–backed service implementation
├── Shared/
│   ├── MainLayout.razor           # App shell layout
│   └── NavMenu.razor              # Sidebar navigation
├── Views/                         # Legacy MVC views (unused, kept as scaffold)
├── wwwroot/
│   ├── css/site.css               # Custom styles (cards, badges, sidebar, etc.)
│   └── lib/                       # Bootstrap 5, jQuery, jquery-validation
├── App.razor                      # Blazor router root
├── _Imports.razor                 # Global Blazor using statements
├── appsettings.json               # App config (connection string, logging)
├── Program.cs                     # App startup and DI configuration
└── InventorySystem.csproj         # Project file
```

---

## Data Models

### Product
The central entity representing an inventory item.

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `ProductCode` | `string` (max 50) | Auto-generated as `PRD-<ticks>` |
| `ProductName` | `string` (max 200) | Required |
| `HSNCode` | `string` (max 100) | Harmonised System of Nomenclature code |
| `TotalStock` | `decimal` | Running stock total |
| `Active` | `bool` | Soft active/inactive flag |
| `IsFavourite` | `bool` | Favourite flag |
| `CreatedDate` | `DateTimeOffset` | UTC creation timestamp |
| `UpdatedDate` | `DateTimeOffset` | UTC last-update timestamp |
| `CreatedUser` | `Guid` | User reference (not enforced via FK) |
| `Variants` | `ICollection<ProductVariant>` | Navigation property |
| `StockTransactions` | `ICollection<StockTransaction>` | Navigation property |

### ProductVariant
A named variant group belonging to a product (e.g. "Size", "Color").

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `Name` | `string` | e.g. "Size" |
| `ProductId` | `Guid` | FK → Product (cascade delete) |
| `Options` | `ICollection<VariantOption>` | Navigation property |

### VariantOption
A single option value within a variant (e.g. "S", "M", "L").

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `OptionValue` | `string` | e.g. "M" |
| `ProductVariantId` | `Guid` | FK → ProductVariant (cascade delete) |

### StockTransaction
An immutable audit record for every stock movement.

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `ProductId` | `Guid` | FK → Product (cascade delete) |
| `Quantity` | `decimal` | Units moved |
| `TransactionType` | `string` | `"PURCHASE"` or `"SALE"` |
| `CreatedDate` | `DateTime` | UTC timestamp |

---

## API Reference

Base URL: `https://localhost:7289/api`

Swagger UI is available at `https://localhost:7289/swagger` when running in Development mode.

### Products

#### `POST /api/product`
Creates a new product with optional variants.

**Request body:**
```json
{
  "name": "Shirt",
  "hsnCode": "6205",
  "variants": [
    {
      "name": "Size",
      "options": ["S", "M", "L", "XL"]
    },
    {
      "name": "Color",
      "options": ["Red", "Blue"]
    }
  ]
}
```

**Response:** `200 OK` — the created `Product` object.  
**Error:** `400 Bad Request` — if `name` is empty.

---

#### `GET /api/product?page=1&pageSize=10`
Returns a paginated list of products including their variants and options.

**Query parameters:**

| Parameter | Default | Description |
|---|---|---|
| `page` | `1` | Page number (1-based) |
| `pageSize` | `10` | Items per page |

**Response:** `200 OK` — array of `Product` objects with nested `Variants` and `Options`.

---

#### `POST /api/product/add-stock?productId={guid}&qty={decimal}`
Adds stock to a product and records a `PURCHASE` transaction.

**Query parameters:**

| Parameter | Description |
|---|---|
| `productId` | GUID of the target product |
| `qty` | Quantity to add (must be > 0) |

**Response:** `200 OK` — updated `Product` object.  
**Error:** `404 Not Found` — if product does not exist.

---

#### `POST /api/product/remove-stock?productId={guid}&qty={decimal}`
Removes stock from a product and records a `SALE` transaction. Prevents stock from going negative.

**Query parameters:**

| Parameter | Description |
|---|---|
| `productId` | GUID of the target product |
| `qty` | Quantity to remove |

**Response:** `200 OK` — updated `Product` object.  
**Errors:** `404 Not Found` / `400 Bad Request` (`"Insufficient stock"`).

---

## Blazor Pages

| Route | Page | Description |
|---|---|---|
| `/` | `Index.razor` | Dashboard with summary cards and recent products table |
| `/products` | `ProductList.razor` | Full paginated product list with Add/Remove Stock action buttons |
| `/create-product` | `CreateProduct.razor` | Form to create a product with dynamic variants and options |
| `/add-stock` | `AddStock.razor` | Select a product and enter a quantity to add |
| `/add-stock/{ProductId}` | `AddStock.razor` | Pre-selects the product from the URL parameter |
| `/remove-stock` | `RemoveStock.razor` | Select a product and enter a quantity to remove |
| `/remove-stock/{ProductId}` | `RemoveStock.razor` | Pre-selects the product from the URL parameter |

All pages communicate with the API via an injected `HttpClient` with base address `https://localhost:7289/`.

---

## Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or higher) — the default connection string targets a local `SQLEXPRESS` instance
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or VS Code with C# extension

---

## Getting Started

### 1. Clone / Extract the project

```bash
# If cloning from a repo:
git clone <repository-url>
cd InventorySystem

# Or extract the zip and open the solution:
# InventorySystem.sln
```

### 2. Restore NuGet packages

```bash
dotnet restore
```

### 3. Configure the database connection

Open `appsettings.json` and update the `DefaultConnection` string to match your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER\\SQLEXPRESS;Database=InventoryDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Common values for `Server`:
- Local default instance: `.` or `localhost`
- Local named instance: `.\SQLEXPRESS` or `HP\SQLEXPRESS`
- Remote server: `myserver.domain.com`

### 4. Apply database migrations

```bash
dotnet ef database update
```

This runs the `InitialCreate` migration and creates the `InventoryDB` database with all four tables (`Products`, `ProductVariants`, `VariantOptions`, `StockTransactions`).

> If `dotnet ef` is not found, install the EF Core CLI tools:
> ```bash
> dotnet tool install --global dotnet-ef
> ```

### 5. Run the application

```bash
dotnet run
```

Or press **F5** in Visual Studio.

---

## Database Setup

The migration creates the following schema:

```
Products
  Id (uniqueidentifier, PK)
  ProductCode, ProductName, HSNCode (nvarchar)
  TotalStock (decimal 18,2)
  Active, IsFavourite (bit)
  CreatedDate, UpdatedDate (datetimeoffset)
  CreatedUser (uniqueidentifier)

ProductVariants
  Id (uniqueidentifier, PK)
  Name (nvarchar)
  ProductId (uniqueidentifier, FK → Products, CASCADE)

VariantOptions
  Id (uniqueidentifier, PK)
  OptionValue (nvarchar)
  ProductVariantId (uniqueidentifier, FK → ProductVariants, CASCADE)

StockTransactions
  Id (uniqueidentifier, PK)
  ProductId (uniqueidentifier, FK → Products, CASCADE)
  Quantity (decimal 18,2)
  TransactionType (nvarchar)   -- "PURCHASE" or "SALE"
  CreatedDate (datetime2)
```

---

## Configuration

### `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=HP\\SQLEXPRESS;Database=InventoryDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### `appsettings.Development.json`

Overrides logging to `Debug` level for development.

### CORS

The app registers a `"AllowBlazor"` CORS policy in `Program.cs` that permits any origin, header, and method. Restrict this for production:

```csharp
policy.WithOrigins("https://yourdomain.com")
      .AllowAnyHeader()
      .AllowAnyMethod();
```

### HTTP Client Base Address

The Blazor `HttpClient` is registered with base address `https://localhost:7289/`. Update this in `Program.cs` if your port changes:

```csharp
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("https://localhost:YOUR_PORT/") });
```

---

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | 7.0.1 | SQL Server EF Core provider |
| `Microsoft.EntityFrameworkCore.Design` | 7.0.1 | EF Core CLI tooling support |
| `Microsoft.EntityFrameworkCore.Tools` | 7.0.1 | EF Core migrations tooling |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | 12.0.1 | Object-to-object mapping |
| `Serilog.AspNetCore` | 7.0.0 | Structured logging |
| `Swashbuckle.AspNetCore` | 7.1.0 | Swagger / OpenAPI documentation |

Front-end libraries bundled in `wwwroot/lib/`:
- **Bootstrap 5**
- **jQuery 3**
- **jquery-validation** + **jquery-validation-unobtrusive**
