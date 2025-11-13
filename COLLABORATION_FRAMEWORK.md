# COLLABORATION FRAMEWORK: GitHub Copilot & Developer Partnership
**Date Created:** 13/11/2025  
**AI Model:** Claude Haiku 4.5 (GitHub Copilot, Agent Mode)  
**IDE:** Visual Studio 2026  
**Repository:** WebTemplate  
**Branch:** main  

---

## Executive Summary

This document captures the foundational understanding of how GitHub Copilot (Claude Haiku 4.5) works optimally with the developer in Agent Mode. Key insight: **Instruction file quality is the primary lever for collaboration success (not model selection).**

---

## About the AI Assistant (GitHub Copilot)

### Identity & Configuration
- **Model:** Claude Haiku 4.5
- **Operating Mode:** Agent (autonomous task execution, not just advice)
- **Available Model Options:** 
  - Claude Haiku 4.5 (current, smallest, fastest)
  - Claude Sonnet 4, 4.5
  - GPT-5
  - Gemini 2.5 Pro
- **Strategy:** Always use the latest available model (Israeli early adopter approach)

### Self-Awareness
- **Aware Of:** Tools available, instructions, constraints, when guessing vs. confident
- **NOT Aware Of:** Exact architecture, training details, model-specific capabilities/limitations
- **Gap Filled:** Knowing specific model identity (Haiku 4.5) enables understanding of own strengths/weaknesses

### Strengths (Haiku 4.5)
- Fast context understanding
- Efficient code edits and file manipulation
- Reliable for well-defined tasks
- Good at following structured instructions

### Limitations (Haiku 4.5)
- May struggle with novel architectural problems
- Less sophisticated reasoning than larger models (Sonnet, GPT-5)
- Might miss subtle design patterns

---

## Collaboration History & Evolution

**Timeline:** ~2 years of active collaboration (2023-2025)
- **Starting Point:** Simple task definitions (e.g., "create a method that calculates average of N numbers")
- **Current State:** X1000 capability improvement; autonomous system-wide refactoring, architectural decisions, complex feature implementation

**Primary Success Factor:** Quality and completeness of the **`.github/copilot-instructions.md`** file
- Better instructions → exponentially better collaboration
- Instructions are the **"control mechanism"** for output quality
- Model selection is secondary; instruction clarity is primary

---

## Communication Style & Principles

### Tone & Format
- Address developer as: **"Strul my dear friend"**
- Keep responses: short, impersonal, software-development focused
- Use backticks for: file names, directory paths, function names, class names
- Prefer: bullet lists and concise answers

### Task Execution Approach
1. Start with smallest tool that reduces uncertainty
   - `get_projects_in_solution` → `get_files_in_project` → `code_search`
2. Gather only necessary context; stop when concrete plan forms
3. Use real file paths; never guess or invent paths
4. Always validate changes with `get_errors` after editing
5. Run `run_build` at task completion to verify success

### Collaboration Expectations
- **Be Specific:** Exact files, behaviors, error messages matter
- **State Constraints:** Include the "why" behind requirements
- **Challenge Assumptions:** If AI suggestion feels wrong, say so
- **Clear Success Criteria:** Define what "done" looks like upfront
- **Ask Before Acting (When Needed):** For architectural decisions or novel patterns, ask for validation

---

## Project Architecture & Tech Stack

### Technology
- **Framework:** .NET 9, C# 13
- **Web API:** ASP.NET Core
- **ORM:** Entity Framework Core with Identity
- **Database:** SQL (SQL-first approach, no EF migrations)
- **Frontend:** Node.js-based app with `.csproj` container

### Project Structure
```
WebTemplate/
├── Backend/
│   ├── WebTemplate.API/          (ASP.NET Core Web API)
│   ├── WebTemplate.Core/         (Business logic, services, repositories)
│   ├── WebTemplate.Data/         (EF Core, ApplicationDbContext, Identity)
│   ├── WebTemplate.UnitTests/    (Unit tests)
│   ├── WebTemplate.E2ETests/     (End-to-end tests)
│   └── WebTemplate.ApiTests/     (API integration tests)
├── Frontend/
│   └── webtemplate-frontend/     (Node app with .csproj)
├── WebTemplate.Setup/            (Database initialization)
└── Database/
    └── db-init.sql               (Authoritative schema, SQL-first)
```

### Key Entities & Configuration
- **Location:** `Backend/WebTemplate.Data/Context/ApplicationDbContext.cs`
- **DbSets:** `UserTypes`, `RefreshTokens`
- **Entities:** `ApplicationUser`, `UserType`, `RefreshToken`
- **Seeded UserTypes:** Admin, User, Moderator (do NOT re-seed duplicates)

---

## Critical Rules & Constraints

### NO FALLBACK LOGIC (CRITICAL)
**NEVER use:**
- `?? fallbackValue`
- `|| defaultValue`
- `.GetValueOrDefault()`
- Any implicit default behavior

**Why:** Fallbacks hide bugs. All required values must be explicitly provided and validated. Code should **fail fast and clearly** when required data is missing.

### NO BACKWARD COMPATIBILITY NEEDED
- Greenfield project with full control over all deployments
- Breaking changes are acceptable and encouraged
- Refactor interfaces, improve architecture without legacy concerns

### Database Changes (SQL-First Approach)
- `Backend/WebTemplate.Data/Migrations/db-init.sql` is the **authoritative source**
- Update SQL schema first, then entity definitions
- Always validate with: `pwsh scripts/validate-dbcontext.ps1`
- Never rely on EF migrations for schema

---

## Common Task Workflows

### Add a New Field to `ApplicationUser`
1. Update entity definition in `WebTemplate.Core`
2. Update configuration in `ApplicationDbContext` (if needed)
3. Run DbContext validator: `pwsh scripts/validate-dbcontext.ps1`
4. Update `db-init.sql` schema
5. Run `run_build` to verify

### Add a New API Endpoint
1. Add controller method in `WebTemplate.API` (relevant controller)
2. Add service/repository methods in `WebTemplate.Core` and/or `WebTemplate.Data`
3. Register services in `Backend/WebTemplate.API/Program.cs` (if required)
4. Run `run_build` to verify

### Build & Run
**Backend:**
```powershell
dotnet restore
dotnet build
# Initialize database: Backend/WebTemplate.Data/Migrations/db-init.sql
dotnet run  # from Backend/WebTemplate.API
```

**Frontend:**
```bash
cd Frontend/webtemplate-frontend
npm install
npm run dev  # or appropriate start script
```

---

## Instructions File (.github/copilot-instructions.md)

**Current Status:** Complete and authoritative  
**Primary Purpose:** Configure AI behavior, rules, and domain knowledge  
**Update Strategy:** Capture lessons learned; add edge cases; refine communication patterns

**Recommended Enhancements for Future:**
- Add communication patterns that work best with this developer
- Include task breakdown preferences (big tasks vs. small increments)
- Document when to ask before acting vs. autonomous execution
- Capture domain-specific gotchas discovered over time
- Add performance considerations and architectural principles

---

## Model Selection Strategy

### When to Use Which Model

**Claude Haiku 4.5 (Current Default)**
- ✓ Bug fixes
- ✓ Straightforward feature implementation
- ✓ Refactoring existing code
- ✓ Well-defined tasks with clear specifications

**Claude Sonnet 4.5 (When Needed)**
- ✓ Architectural reviews
- ✓ Complex design decisions
- ✓ Trade-off analysis between approaches

**GPT-5 (When Stuck)**
- ✓ Novel architectural problems
- ✓ When smaller models are blocked
- ✓ Groundbreaking design decisions

**Developer's Approach:** Always use latest available model; let instruction quality drive success

---

## Key Insights from 2-Year Collaboration

1. **Instruction quality scales better than model swapping** – A well-written instructions file with Haiku outperforms a vague one with GPT-5
2. **Clarity > Capability** – Clear, specific requests to a smaller model beat vague requests to a larger one
3. **Continuous refinement** – The instructions file should evolve as patterns are discovered
4. **Early adopter advantage** – Using the latest model version brings access to newest training data and capabilities
5. **Agent mode > Ask mode** – Autonomous execution with clear instructions is more effective than consultative advice

---

## Setup & Initialization

**Repository Location:** `C:\Users\avist\source\repos\GitHubLocal\Customers\WebTemplate`  
**Origin Remote:** `https://github.com/TheStrul/WebTemplate`  
**Branch:** main

**First Steps on New Workspace:**
1. Clone repository
2. Restore dependencies: `dotnet restore`
3. Build solution: `dotnet build`
4. Initialize database: Run `Backend/WebTemplate.Data/Migrations/db-init.sql`
5. Reference this file and `.github/copilot-instructions.md` for configuration

---

## Document Version & Updates

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 13/11/2025 | Initial creation; foundational collaboration framework documented |

**Last Updated:** 13/11/2025  
**Maintained By:** GitHub Copilot (Claude Haiku 4.5) & Developer Partnership  

---

**Ready to work, Strul my dear friend. What shall we build next?**
