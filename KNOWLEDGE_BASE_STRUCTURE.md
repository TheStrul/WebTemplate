# TheStrul Institutional Memory - Knowledge Base Structure

**Purpose:** This document defines the structure and contents of the Institutional Memory Knowledge Base – a centralized repository capturing 2+ years of development patterns, architectural decisions, and best practices.

**Status:** Foundation Document (Version 1.0)  
**Created:** 13/11/2025  
**Scope:** .NET 9 Web Applications with Azure integration

---

## Overview

The Knowledge Base will be organized as a separate GitHub repository (`TheStrul/knowledge-base`) containing:

1. **Architecture Patterns & Decisions**
2. **Code Patterns & Best Practices**
3. **Project Templates & Scaffolds**
4. **Configuration Strategies**
5. **Testing Patterns**
6. **Deployment & DevOps**
7. **Common Gotchas & Solutions**
8. **Tool & Library Standards**

---

## Proposed Directory Structure

```
knowledge-base/
├── README.md                           (Overview & quick links)
├── QUICK_START.md                      (How to use this KB)
│
├── 1_ARCHITECTURE/
│   ├── 01_PRINCIPLES.md               (Core architectural principles)
│   ├── 02_LAYERED_ARCHITECTURE.md     (API/Core/Data layer patterns)
│   ├── 03_DOMAIN_PATTERNS.md          (Entity modeling, aggregates, value objects)
│   ├── 04_ERROR_HANDLING.md           (Result pattern, Error model)
│   ├── 05_CONFIGURATION.md            (Configuration strategy, NO FALLBACKS rule)
│   └── examples/
│       ├── result-pattern.cs
│       ├── configuration-example.cs
│       └── error-handling-flow.cs
│
├── 2_PATTERNS/
│   ├── 01_SERVICE_LAYER.md            (Service implementations, responsibilities)
│   ├── 02_REPOSITORY_PATTERN.md       (Data access, EF Core usage)
│   ├── 03_DTO_PATTERNS.md             (Data transfer objects, mapping)
│   ├── 04_API_CONTROLLERS.md          (REST API design, routing)
│   ├── 05_FEATURE_MODULES.md          (Modular feature loading)
│   ├── 06_DEPENDENCY_INJECTION.md     (DI container setup, registration)
│   └── examples/
│       ├── service-implementation.cs
│       ├── repository-implementation.cs
│       ├── controller-endpoint.cs
│       └── feature-module.cs
│
├── 3_DATABASE/
│   ├── 01_SQL_FIRST_APPROACH.md       (Why SQL-first, schema-first design)
│   ├── 02_EF_CONFIGURATION.md         (Entity configuration, DbContext setup)
│   ├── 03_MIGRATIONS_STRATEGY.md      (No EF migrations, manual SQL)
│   ├── 04_IDENTITY_INTEGRATION.md     (ApplicationUser, Identity setup)
│   ├── 05_SEED_DATA.md                (Seeding strategy, initial data)
│   └── examples/
│       ├── schema-example.sql
│       ├── dbcontext-configuration.cs
│       └── entity-definition.cs
│
├── 4_TESTING/
│   ├── 01_UNIT_TESTING.md             (Unit test structure, mocking)
│   ├── 02_API_TESTING.md              (Integration tests, test fixtures)
│   ├── 03_E2E_TESTING.md              (End-to-end test scenarios)
│   ├── 04_TEST_DATA.md                (Test data builders, factories)
│   └── examples/
│       ├── unit-test-example.cs
│       ├── api-test-example.cs
│       └── test-fixture-example.cs
│
├── 5_SECURITY/
│   ├── 01_AUTHENTICATION.md           (JWT tokens, refresh tokens)
│   ├── 02_AUTHORIZATION.md            (Role-based access, user types)
│   ├── 03_SECURE_CONFIGURATION.md     (Secrets, Key Vault, environment)
│   ├── 04_SECURITY_HEADERS.md         (CORS, CSP, security headers)
│   └── examples/
│       ├── auth-flow.md
│       └── token-service-pattern.cs
│
├── 6_FRONTEND/
│   ├── 01_PROJECT_STRUCTURE.md        (Frontend folder layout)
│   ├── 02_API_INTEGRATION.md          (Calling backend APIs)
│   ├── 03_STATE_MANAGEMENT.md         (State patterns)
│   └── 04_DEPLOYMENT.md               (Build & deployment)
│
├── 7_DEVOPS/
│   ├── 01_ENVIRONMENTS.md             (Development, Staging, Production)
│   ├── 02_BUILD_PROCESS.md            (dotnet build, versioning)
│   ├── 03_DEPLOYMENT.md               (Azure deployment, CI/CD)
│   ├── 04_LOGGING.md                  (Serilog, Application Insights)
│   └── 05_MONITORING.md               (Health checks, performance tracking)
│
├── 8_COMMON_TASKS/
│   ├── 01_ADD_NEW_ENTITY.md           (Step-by-step: new database entity)
│   ├── 02_ADD_NEW_ENDPOINT.md         (Step-by-step: new API endpoint)
│   ├── 03_ADD_NEW_SERVICE.md          (Step-by-step: new service)
│   ├── 04_UPDATE_SCHEMA.md            (Step-by-step: database schema changes)
│   ├── 05_AUTHENTICATION_FLOW.md      (Understanding auth system)
│   └── 06_ERROR_HANDLING.md           (Error handling strategy)
│
├── 9_GOTCHAS_AND_LESSONS/
│   ├── 01_LESSONS_LEARNED.md          (Mistakes & how to avoid them)
│   ├── 02_PERFORMANCE_TIPS.md         (Optimization patterns)
│   ├── 03_EF_CORE_PITFALLS.md         (EF Core gotchas)
│   ├── 04_ASYNC_PATTERNS.md           (Async/await best practices)
│   ├── 05_VALIDATION_STRATEGY.md      (Input validation patterns)
│   └── 06_CONFIGURATION_PITFALLS.md   (Configuration gotchas)
│
├── 10_TOOLS_AND_LIBRARIES/
│   ├── 01_NUGET_PACKAGES.md           (Approved packages & versions)
│   ├── 02_EXTERNAL_SERVICES.md        (Azure, SMTP, Key Vault, etc.)
│   ├── 03_DEVELOPMENT_TOOLS.md        (IDE config, linters, formatters)
│   └── 04_VALIDATION_TOOLS.md         (DbContext validator, etc.)
│
├── 11_TEMPLATES/
│   ├── .github-copilot-instructions-template.md
│   ├── Program.cs.template
│   ├── appsettings.Local.json.template
│   ├── controller.template.cs
│   ├── service.template.cs
│   ├── repository.template.cs
│   ├── entity.template.cs
│   └── dto.template.cs
│
└── CHANGELOG.md                        (Version history & updates)
```

---

## Document Templates

Each section will follow this structure:

### Architecture/Pattern Documents
```markdown
# [Pattern/Architecture Name]

## Overview
- What is this pattern?
- Why do we use it?
- When should it be applied?

## Key Principles
- Principle 1
- Principle 2
- Principle 3

## Implementation Guide
- Step 1
- Step 2
- Step 3

## Code Example
[Actual code snippet]

## Common Mistakes
- Mistake 1 & how to avoid
- Mistake 2 & how to avoid

## Related Patterns
- Link to related documents

## Notes & Gotchas
- Important considerations
```

---

## Content to Extract from WebTemplate

### From Architecture
- ✓ Layered architecture (API/Core/Data/Frontend)
- ✓ Configuration strategy (ApplicationConfiguration, NO FALLBACKS)
- ✓ Feature modules (FeatureHost, modular loading)
- ✓ Result/Error pattern (success/failure handling)
- ✓ DI container setup (Program.cs pattern)

### From Services
- ✓ AuthService pattern (authentication flow)
- ✓ UserService pattern (CRUD operations)
- ✓ TokenService pattern (JWT handling)
- ✓ EmailSender pattern (interface + implementations)
- ✓ RefreshTokenCleanupService (background tasks)

### From Data Layer
- ✓ Repository pattern (IRefreshTokenRepository, IUserTypeRepository)
- ✓ DbContext configuration (Entity configuration, seeding)
- ✓ SQL-first approach (db-init.sql authoritative)
- ✓ Identity integration (ApplicationUser, UserType)
- ✓ DbContext validator tool

### From API Layer
- ✓ Controller patterns (AuthController, UserController, UserTypeController)
- ✓ Route organization
- ✓ Request/response handling
- ✓ Error response formatting

### From Testing
- ✓ Unit test structure
- ✓ API test fixtures
- ✓ E2E test scenarios
- ✓ Test data patterns

### From Configuration
- ✓ appsettings structure
- ✓ Environment-specific config
- ✓ Feature toggles (FeaturesOptions)
- ✓ Security settings (JWT, Auth, CORS)

---

## Integration Points

### How Projects Will Use This KB

**For New Projects:**
1. Reference COLLABORATION_FRAMEWORK.md (already in place)
2. Reference KNOWLEDGE_BASE_STRUCTURE.md (this document)
3. Reference specific pattern documents as needed (e.g., "Adding a New Entity")
4. Use templates to scaffold initial project

**For GitHub Copilot (AI):**
1. Access this KB as part of project context
2. Follow patterns documented here
3. Reference examples when writing new code
4. Apply "gotchas" knowledge to prevent issues

**For Human Developers:**
1. Quick reference for architectural decisions
2. Copy/paste code templates
3. Learn "why" behind patterns (decision rationale)
4. See real examples from working code

---

## Next Steps

1. **Create GitHub Repository:** `TheStrul/knowledge-base`
2. **Populate Sections 1-5:** Core patterns (Architecture, Patterns, Database, Testing, Security)
3. **Extract Code Examples:** From WebTemplate
4. **Create Templates:** For common scaffolding tasks
5. **Document Gotchas:** From 2 years of experience
6. **Version & Release:** Tag v1.0 when complete

---

## Maintenance Strategy

- **Quarterly Reviews:** Every 3 months, update with new learnings
- **Per-Project Updates:** After each customer project, capture new patterns
- **Version Control:** Semantic versioning (v1.0, v1.1, v2.0, etc.)
- **Feedback Loop:** Each project references KB; improvements documented back to KB
- **AI Awareness:** KB kept up-to-date in Copilot context window

---

**Ready to build, Strul my dear friend. Shall we start with Section 1: Architecture Patterns?**
