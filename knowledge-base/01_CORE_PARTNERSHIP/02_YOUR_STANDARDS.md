# YOUR STANDARDS: Technical & Professional

**Last Updated:** 13/11/2025  
**Purpose:** Non-negotiable principles that define collaboration

---

## 1. NO FALLBACK LOGIC (CRITICAL)

### The Rule
```
❌ NEVER USE:
- ?? (null-coalescing)
- || (logical OR for defaults)
- .GetValueOrDefault()
- Silent defaulting of any kind

✅ ALWAYS USE:
- Explicit validation
- Fail fast with clear errors
- Required values must be explicitly provided
- Missing data surfaces immediately
```

### Why
**Fallbacks hide bugs.** Production issues should surface at startup, not in production.

### Examples

**Wrong:**
```csharp
var value = config.GetValue("setting") ?? defaultValue;  // Hides missing config
var timeout = settings.Timeout.GetValueOrDefault();      // Silent default
```

**Right:**
```csharp
var value = config.GetRequiredValue("setting")           // Fails if missing
  ?? throw new ConfigurationException("'setting' is required");
var timeout = settings.Timeout 
  ?? throw new ArgumentException("Timeout must be explicitly configured");
```

### Application
- Code validation
- Configuration loading
- Database queries
- API responses
- Architecture decisions

---

## 2. SQL-FIRST APPROACH

### The Authority Chain
1. **`db-init.sql`** is authoritative (source of truth for schema)
2. Entity definitions follow SQL (not the reverse)
3. EF Core configures based on SQL (no EF migrations for schema)
4. DbContext validator ensures 100% match between SQL and code

### Process for Data Changes
1. Update `Backend/WebTemplate.Data/Migrations/db-init.sql`
2. Update entity in `WebTemplate.Core`
3. Update configuration in `ApplicationDbContext` if needed
4. Run validator: `pwsh scripts/validate-dbcontext.ps1`
5. Verify 100% match before deployment

### Why
SQL is explicit. EF migrations are implicit. Explicit is always better.

---

## 3. EXPLICIT VALIDATION

### Pattern
**Every required value must be validated explicitly.**

```csharp
// Wrong: Silent assumption
public void ProcessUser(User user)
{
    var name = user.Name;  // What if null?
}

// Right: Explicit validation
public void ProcessUser(User user)
{
    if (string.IsNullOrEmpty(user.Name))
        throw new ArgumentException("User.Name is required", nameof(user));
    
    var name = user.Name;  // Now guaranteed
}
```

### Application
- Method parameters
- Configuration values
- Query results
- API inputs
- State transitions

---

## 4. FAIL FAST

### Philosophy
**Better to fail loudly at startup than silently in production.**

### Implementation
- Validate inputs immediately
- Throw on invalid state
- Don't continue with partial data
- Surface errors to the top level

### Example
```csharp
// Wrong: Continue with partial data
public void Initialize(Config config)
{
    this.database = config.ConnectionString ?? "localhost";  // Continues!
}

// Right: Fail immediately
public void Initialize(Config config)
{
    if (string.IsNullOrEmpty(config.ConnectionString))
        throw new ConfigurationException("ConnectionString is required");
    
    this.database = config.ConnectionString;
}
```

---

## 5. GREENFIELD MINDSET

### Principles
- ❌ **NO** backward compatibility concerns
- ✅ **YES** breaking changes if they improve architecture
- ✅ **YES** refactoring for clarity
- ✅ **YES** optimizing for future maintainability

### Application
- Rename APIs when better names emerge
- Restructure modules for clarity
- Remove legacy code paths
- Change interfaces for consistency

### Why
This is a new project with full control. Don't carry legacy baggage.

---

## 6. KNOWLEDGE DEPENDENCY

### Hierarchy
1. **Better instructions** > Better models
2. **Clarity** > Capability
3. **Explicit** > Implicit
4. **Agent Mode** > Ask Mode

### What This Means
- Investment in clear documentation pays more than raw model quality
- Explicit instructions beat hoping the AI understands
- Working autonomously (with clear parameters) beats checking in constantly
- The KB matters more than the model selection

---

## 7. PROJECT STRUCTURE ADHERENCE

### Backend Layout
- `Backend/WebTemplate.API` - ASP.NET Core, .NET 9
- `Backend/WebTemplate.Core` - Business logic, services
- `Backend/WebTemplate.Data` - EF Core, Identity, SQL
- All follow consistent patterns

### Frontend
- `Frontend/webtemplate-frontend` - Node-based app

### Configuration
- No secrets in source
- Use User Secrets or environment variables
- Connection strings in `appsettings.json` (development) or env vars (production)

### Database
- SQL-first: `db-init.sql` is authoritative
- Identity tables: provisioned by SQL script
- Seeding: via SQL or guarded dev seeder (never auto-migrate in production)

---

## 8. NO BACKWARD COMPATIBILITY NEEDED

This is critical: **This is a greenfield project.**

### Implications
- Feel free to make breaking changes
- Refactor interfaces aggressively
- Improve architecture without worrying about legacy
- Optimize for clarity, not compatibility

### Exception
None. Full freedom.

---

## 9. COMMUNICATION CLARITY

### Naming Convention
When discussing roles:
- Format: "I (Strul)" and "You (Murdock)" or "I (Claude Haiku)"
- Purpose: Eliminate ambiguity about who is doing what
- Application: Use rigorously when identity could blur

### Documentation
- Write for clarity, not brevity
- Explain the "why" not just the "what"
- Include examples
- Make assumptions explicit

---

## How These Standards Work Together

They form a **philosophy of explicitness:**
- No fallbacks = no silent failures
- SQL-first = authoritative schema
- Explicit validation = known state
- Fail fast = immediate feedback
- Greenfield = freedom to optimize
- Clear instructions = effective autonomy

**The result:** Code that's hard to misuse, easy to maintain, and honest about its state.

---

**REFERENCE THESE STANDARDS** when making any architectural or coding decision.
