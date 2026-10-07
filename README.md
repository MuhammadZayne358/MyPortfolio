
# MyPortfolio

A full-stack luxury watch e-commerce web application built with **ASP.NET Core MVC (.NET 9)**, Entity Framework Core and SQL Server. It includes a customer-facing storefront and an admin panel for managing products, orders and contact messages.

> Built as a portfolio project to demonstrate end-to-end backend and frontend work.

## Features

**Storefront**
- Product listing and product detail pages
- AJAX cart (side panel + dedicated cart page)
- Checkout with payment method selection (Cash on Delivery, JazzCash, EasyPaisa)
- Order history ("My Orders")
- Contact form (messages are stored and visible to the admin)
- About and Privacy pages

**Authentication**
- Sign up / login / logout using ASP.NET Core Identity
- Forgot and reset password via email (SMTP)
- Role-based access (`Admin` role)

**Admin panel**
- Dashboard
- Add, edit and delete products (with image upload)
- View and manage all orders, including order details
- Read customer contact messages (unread counter in the layout)

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core MVC, .NET 9 |
| Language | C# |
| Database | SQL Server |
| ORM | Entity Framework Core (Code First, Migrations) |
| Auth | ASP.NET Core Identity |
| Frontend | Razor Views, Bootstrap, JavaScript (fetch API) |

## Project Structure

```
MyPortfolio/
├── Controllers/       Home, Cart, Order, Identity controllers
├── Models/            Entities, view models, EmailService
├── Data/              ApplicationDbContext
├── Migrations/        EF Core migrations
├── ViewComponents/    Reusable view components (e.g. unread messages)
├── Views/             Razor views (Home, Cart, Order, Identity, Shared)
└── wwwroot/           CSS, JS, images
```

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express or full)

### 1. Clone the repo

```bash
git clone https://github.com/MuhammadZayne358/MyPortfolio.git
cd MyPortfolio/MyPortfolio
```

### 2. Configure secrets

Secrets are **not** stored in `appsettings.json`. They are kept in .NET User Secrets, so you need to add your own values:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=MyPortfolioDb;Trusted_Connection=True;TrustServerCertificate=True"

dotnet user-secrets set "EmailSettings:SenderEmail" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:SenderPassword" "your-gmail-app-password"

dotnet user-secrets set "Admin:Email" "admin@example.com"
dotnet user-secrets set "Admin:Password" "YourStrongPassword123!"
```

Notes:
- `SenderPassword` should be a Gmail **App Password**, not your normal password.
- The admin account is created automatically on first run from `Admin:Email` and `Admin:Password`.
- The password must meet Identity's default rules (uppercase, lowercase, digit, symbol, 6+ characters).

### 3. Create the database

```bash
dotnet ef database update
```

(If you don't have the EF tool: `dotnet tool install --global dotnet-ef`)

### 4. Run

```bash
dotnet run
```

Then open the URL shown in the terminal (by default `https://localhost:7263`).

## Admin Access

Log in with the admin email and password you set in step 2. The admin panel (dashboard, products, orders, messages) becomes available for that account.

## Author

**Zain**
GitHub: [@MuhammadZayne358](https://github.com/MuhammadZayne358)

## License

This project is for portfolio and learning purposes.
