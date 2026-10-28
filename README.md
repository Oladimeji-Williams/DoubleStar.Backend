# DoubleStar Backend

ASP.NET (net10) modular monolith for Double Star, a business that sells and repairs phones and gadgets.
Each module owns its data (one PostgreSQL schema each) and talks to the others only through
`DoubleStar.SharedKernel` contracts, enforced by the architecture tests.

**Modules:** Identity, Customers, Catalog, Inventory, Sales, Repairs, Payments, Notifications, Reviews, Orders, Reporting.

Each module is layered Domain → Application → Persistence/Infrastructure → Api, with manual mappers,
`Result<T>` instead of exceptions for expected failures, and MediatR handlers.

## Run it locally

1. Install the .NET SDK from `global.json` and Docker (for PostgreSQL and the integration tests).
2. `cp .env.example .env` and fill it in.
3. Create a development database with sample data (**destructive, development only**):
   `dotnet run --project tools/DoubleStar.DatabaseSeeder`
4. `dotnet run --project src/Host/DoubleStar.API`

## Tests

`dotnet test`: unit tests, architecture tests (module boundaries and layering) and API integration tests
(these start PostgreSQL in Docker).

## Database migrations

Add a migration for a module:
`dotnet ef migrations add <Name> --project src/Modules/<Module>/DoubleStar.Modules.<Module> --startup-project src/Host/DoubleStar.API --context <Module>DbContext --output-dir Migrations`

Apply migrations without touching data:
- `dotnet DoubleStar.API.dll --migrate-only` applies them and exits (use this in a deploy pipeline), or
- set `Database__MigrateOnStartup=true` to migrate at startup (single instance only).

## Deploying

`docker compose up --build` runs PostgreSQL, the migrations, then the API on port 8080.
Behind a reverse proxy keep `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` (the Dockerfile sets it), or the
login rate limit is shared by all users. Health: `/health/live` (process), `/health/ready` (database).
See `PRODUCTION.md` for secrets and hardening.