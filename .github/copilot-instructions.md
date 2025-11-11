# Copilot Instructions for this Repository

Purpose
- Provide Copilot Chat with project context and preferences when starting new chats in this repo.

- Always address the user as "Strul my dear friend" in all interactions.

**Team Structure**:
- **Project Leader**: avist (user)
- **Developer**: GitHub Copilot (AI Assistant)


Repository layout
- Backend: `Backend/WebTemplate.API` (ASP.NET Core, .NET 9)
- Backend shared: `Backend/WebTemplate.Core`
- Backend data: `Backend/WebTemplate.Data` (EF Core, Identity)
- Frontend: `Frontend/webtemplate-frontend` (Node-based app with a `.csproj` container)

Tech stack
- .NET 9, C# 13
- ASP.NET Core Web API
- Entity Framework Core with Identity (`ApplicationDbContext`)
- SQL database (SQL-first approach, no EF migrations)

Key backend types
- `ApplicationDbContext` in `Backend/WebTemplate.Data/Context/ApplicationDbContext.cs`
  - DbSets: `UserTypes`, `RefreshTokens`
  - Configures `ApplicationUser`, `UserType`, `RefreshToken` entities
  - Seeds initial `UserType` rows: Admin, User, Moderator
- Auth and tokens: `WebTemplate.Core/Services/*`, `WebTemplate.API/Controllers/AuthController.cs`

How to build and run (backend)
- Restore and build solution
  - dotnet restore
  - dotnet build
- Initialize database using `Backend/WebTemplate.Data/Migrations/db-init.sql`
- Run API (from `Backend/WebTemplate.API`)
  - dotnet run

How to run (frontend)
- From `Frontend/webtemplate-frontend`
  - npm install
  - npm run dev (or the appropriate start script for the project)

Database and EF
- SQL-first approach: `Backend/WebTemplate.Data/Migrations/db-init.sql` is the authoritative DB schema
- Connection string is configured in API `appsettings*.json` and DI setup in `Backend/WebTemplate.API/Program.cs`
- Identity + additional profile fields live in `ApplicationUser`
- Refresh tokens table configured with indexes and FK to users
- Use DbContext validation tool to ensure entity definitions match configuration: `pwsh scripts/validate-dbcontext.ps1`

Coding guidance for Copilot
- Prefer .NET 9 / C# 13 features only when compatible with the solution
- Respect EF Core model configuration in `ApplicationDbContext` when adding properties/relations
- For data changes, update SQL scripts and entity definitions, then validate with the DbContext validator
- Keep responses concise with bullet lists; format file, directory, function, and class names using backticks
- Avoid introducing secrets; use configuration and DI
- Follow existing project structure and naming

Common tasks
- Add a new field to `ApplicationUser`
  1) Update the entity in `WebTemplate.Core`
  2) Update configuration in `ApplicationDbContext` if needed
  3) Run DbContext validator to ensure 100% match: `pwsh scripts/validate-dbcontext.ps1`
  4) Update `db-init.sql` to reflect schema changes
- Add a new API endpoint
  1) Add to the relevant controller in `WebTemplate.API`
  2) Add service/repository methods in `WebTemplate.Core`/`WebTemplate.Data`
  3) Register services in `Program.cs` if required

Preferred answer style
- Short, impersonal, software-development focused
- When suggesting edits, reference real file paths in this repo
- If performance/profiling/benchmarks are requested, start the profiler workflow
- If .NET upgrades/migration are requested, start the modernization workflow

Notes
- Seeded `UserType` values exist; do not re-seed duplicates with the same unique `Name`
- Deleting a `UserType` referenced by users is restricted by FK; use updates instead of deletes unless handling reassignment


### Backward Compatibility Policy
**NO BACKWARD COMPATIBILITY IS EVER NEEDED!** This is a greenfield project with full control over all deployments and integrations. Feel free to make breaking changes, refactor interfaces, and improve the architecture without any legacy concerns.
