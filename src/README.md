# Portfolio Management System

A modern **Portfolio Management System (PMS)** built with .NET, designed to help investors track, analyze, and manage their financial assets efficiently. Supports multiple accounts, holdings, transactions, and reporting in various currencies.

## Features

* **Multi-account management** – Track multiple investment accounts in one place.
* **Holdings and transactions** – Manage all securities, cash positions, and their movements.
* **Asset pricing and FX** – Automatic handling of asset prices and currency conversions.
* **Portfolio valuation** – Compute account and portfolio value in reporting currency.
* **Transaction cost tracking** – Capture commissions, fees, and realized gains.
* **Extensible architecture** – Designed with DDD principles and EF Core for easy maintenance.

## Tech Stack

* **Backend:** .NET 9, C#, EF Core
* **Database:** SQLite
* **Domain Driven Design:** Entities, Value Objects, Repositories
* **API Layer:** ASP.NET Core Web API
* **Logging & Middleware:** Centralized exception handling and request logging

## Getting Started

### Prerequisites

* .NET 9 SDK
* SQLite
* VS Code

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/your-username/portfolio-management-system.git
   cd portfolio-management-system
   ```
2. Restore dependencies:

   ```bash
   dotnet restore
   ```
3. Apply migrations and create the database:

   ```bash
   dotnet ef database update
   ```
4. Run the application:

   ```bash
   dotnet run --project PM.API
   ```

### Usage

* Access the API via `http://localhost:5000/api`
* Create accounts, add holdings, and record transactions.
* Use built-in endpoints to retrieve portfolio value, holdings, and transaction history.

## Project Structure

```
PM.Domain          # Core domain entities and value objects
PM.Infrastructure  # EF Core configurations and data access
PM.API             # Web API layer
PM.DTO             # Data Transfer Objects
PM.SharedKernel    # Common utilities and base classes
```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for improvements, bug fixes, or new features.

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.

---

