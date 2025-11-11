# Backend Setup and Run Guide

Projects
- API: `Backend/WebTemplate.API`
- Core: `Backend/WebTemplate.Core`
- Data: `Backend/WebTemplate.Data`

Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB/Express/Azure SQL) or another EF Core supported provider
- Optional: `dotnet-ef` CLI (install: `dotnet tool install -g dotnet-ef`)

Configuration
- Create `Backend/WebTemplate.API/appsettings.Development.json` (do not commit secrets):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WebTemplateDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": { "SecretKey": "<dev-secret>", "Issuer": "WebTemplate", "Audience": "WebTemplate.Client" },
  "AuthSettings": { "User": { "RequireConfirmedEmail": true } },
  "Email": { "Provider": "Smtp", "From": "no-reply@example.com", "SmtpHost": "smtp.example.com", "SmtpPort": 587 }
}
```

Database (SQL-first)
- Use the checked-in SQL script to create/init the DB: `Backend/WebTemplate.Data/Migrations/db-init.sql`
- Apply it in SSMS/Azure Data Studio or via `sqlcmd`.
- Do NOT run code-based migrations or `EnsureCreated` in production; startup does not auto-migrate.
- For schema changes, update SQL and regenerate the script or provide an upgrade script.

Build and run API
- From repo root:
  - `dotnet restore`
  - `dotnet build`
- Run API: `dotnet run --project Backend/WebTemplate.API`
- Swagger: `https://localhost:<port>/swagger`

Notes
- Identity tables are provisioned by the SQL script; roles/users can be seeded via SQL or a guarded dev seeder.
- Keep `db-init.sql` authoritative; remove or ignore EF migration calls in pipelines.

### Backward Compatibility Policy
**NO BACKWARD COMPATIBILITY IS EVER NEEDED!** This is a greenfield project with full control over all deployments and integrations. Feel free to make breaking changes, refactor interfaces, and improve the architecture without any legacy concerns.
