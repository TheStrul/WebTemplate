# WebTemplate Project Overview

**Last Updated:** 13/11/2025  
**Purpose:** Project context and why we're building this

---

## What We're Building

WebTemplate is a greenfield ASP.NET Core 9 / .NET 9 project that serves as a reference implementation for:
- ✅ SQL-first database design
- ✅ Clean architecture patterns
- ✅ Identity & authentication
- ✅ Explicit validation (NO FALLBACKS)
- ✅ Production-ready standards

**Tech Stack:**
- Backend: ASP.NET Core 9, .NET 9
- Database: SQL Server, SQL-first
- Frontend: React-based, separate project
- Testing: Unit, Integration, E2E

---

## Why It Matters

This project codifies your standards:
- How you approach architecture
- How you handle data and validation
- How you build for clarity
- How you ensure quality

Every pattern here should exemplify your approach.

---

## Project Structure

### Backend
```
Backend/
├── WebTemplate.API/          - ASP.NET Core API
├── WebTemplate.Core/         - Business logic & services
├── WebTemplate.Data/         - EF Core, Identity, SQL
├── WebTemplate.ApiTests/     - Integration tests
├── WebTemplate.E2ETests/     - End-to-end tests
└── WebTemplate.UnitTests/    - Unit tests
```

### Frontend
```
Frontend/
└── webtemplate-frontend/     - React app
```

---

## Key Files

- `Backend/WebTemplate.Data/Context/ApplicationDbContext.cs` - Entity configuration
- `Backend/WebTemplate.Data/Migrations/db-init.sql` - Authoritative schema
- `Backend/WebTemplate.API/Program.cs` - DI setup, configuration
- `.github/copilot-instructions.md` - Project-specific rules

---

## Next: Read Architecture Patterns

See `02_ARCHITECTURE_PATTERNS.md` for how the code is organized.
