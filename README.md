# WebTemplate

Backend and frontend solution.

- Backend: `Backend/WebTemplate.API` (ASP.NET Core, .NET 9)
- Core: `Backend/WebTemplate.Core`
- Data: `Backend/WebTemplate.Data`
- Frontend: `Frontend/webtemplate-frontend`

Quick start (backend)
- Prereqs: .NET 9 SDK, SQL Server (or change provider), optional `dotnet-ef` tool
- Configure: see `Backend/README.md`
- Build: `dotnet restore && dotnet build`
- DB: from `Backend/WebTemplate.Data` run migrations, then `dotnet ef database update`
- Run API: `dotnet run` in `Backend/WebTemplate.API`

Contributing
- Follow `.editorconfig` and keep changes small and focused.
- No secrets in source. Use User Secrets or environment variables.
