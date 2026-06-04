# AGENTS.md

## Cursor Cloud specific instructions

### Product overview

Single ASP.NET Core MVC app (**FIGusta / Canteen Reservation System**) under `CanteenReservationSystem/`. It uses **SQL Server** (not the committed `canteen.db` SQLite files) via `DefaultConnection` in `appsettings.json` (Windows integrated auth — unsuitable on Linux).

### Prerequisites (not in update script)

- **.NET 10 SDK** — install script at https://dot.net/v1/dotnet-install.sh into `$HOME/.dotnet`; `DOTNET_ROOT` and PATH are in `~/.bashrc`.
- **SQL Server** — run locally, e.g. Docker:
  ```bash
  sudo docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
    -p 1433:1433 --name canteen-sql -d mcr.microsoft.com/mssql/server:2022-latest
  ```
- Override the connection string for Linux/cloud (required):
  ```bash
  export ConnectionStrings__DefaultConnection='Server=localhost,1433;Database=CanteenDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True'
  ```

### Database schema

- `dotnet ef database update` often **fails** on a fresh database: pending model changes (EF 10) and/or migration `20260508123326_InitialSchema` assuming objects that do not exist after `InitialCreate`.
- For a clean dev DB, use **`EnsureCreated`** from the current model (see `/tmp/migrate-runner` pattern used during setup) or apply migrations against an existing team database snapshot.
- After schema creation, seed data manually if the menu is empty.

### Run the web app (development)

From `CanteenReservationSystem/CanteenReservationSystem`:

```bash
export ConnectionStrings__DefaultConnection='Server=localhost,1433;Database=CanteenDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True'
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --launch-profile http --no-launch-browser
```

Default HTTP URL: **http://localhost:5162** (`Properties/launchSettings.json`). Home redirects to `/Dish` (menu).

Use **tmux** for long-running `dotnet run` (see cloud agent shell rules).

### Tests

```bash
cd CanteenReservationSystem && dotnet test
```

As of setup, **`CanteenReservationSystem.Tests` does not build**: fake services in `OrdersControllerTests` and `PollControllerTests` are missing newer `IOrderService` / `IPollService` members. Service-level tests using EF InMemory (e.g. `DishServiceTests`) are fine once the test project compiles.

### Lint / format

No dedicated linter config in-repo. `dotnet build` on the web project is the primary compile check.
