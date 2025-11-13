# Best Practices for AI Knowledge Base Design

**Date:** 2025-11-13  
**Purpose:** Document proven patterns for building effective knowledge bases for AI assistants  
**Status:** Research-based synthesis of LLM context optimization patterns

---

## Executive Summary

An effective AI knowledge base balances **accessibility for AI** with **usability for humans**. Key finding: structure matters more than volume.

---

## 1. Hierarchical & Semantic Organization

### Principle
Organize content by **semantic similarity** (what it's about) rather than by implementation details.

### Anti-Pattern ❌
```
/database
  /migrations
  /queries
  /schemas
  /updates
```

### Best Practice ✅
```
/database-fundamentals
  /sql-first-approach.md
  /entity-definition.md
  /schema-management.md
/database-operations
  /read-patterns.md
  /write-patterns.md
  /transaction-handling.md
```

### Why This Matters
- AI assistants retrieve content using semantic search
- Grouping by problem domain increases retrieval accuracy
- Reduces context window pollution with irrelevant files

---

## 2. Document Size & Granularity

### Principle
**Smaller, focused documents outperform large monoliths.**

### Recommendation
- **Optimal size:** 500-2000 words per document
- **Maximum:** 3000 words (before splitting)
- **Minimum:** 100 words (combine if smaller)

### Why This Matters
- AI context windows are limited (even with 200k window)
- Small documents allow precise retrieval
- Reduces noise during semantic search
- Easier to version and update individual pieces

### Example Structure
```
❌ BAD: "Authentication-Complete-Guide.md" (8000 words)
✅ GOOD:
  - authentication-flow.md (400 words)
  - jwt-token-strategy.md (600 words)
  - refresh-token-pattern.md (800 words)
  - common-auth-mistakes.md (500 words)
```

---

## 3. Clear Metadata & Headers

### Principle
**Every document should be self-describing.**

### Template
```markdown
# [Specific Topic Name]

**Purpose**: One sentence explaining why this exists  
**Related**: Links to 2-3 related documents  
**When to Use**: Specific scenarios where this applies  
**Anti-Pattern**: What NOT to do  

## Overview
- Point 1
- Point 2
- Point 3

## Implementation Guide

[Detailed steps]

## Code Example
[Real, tested code]

## Common Mistakes
- Mistake 1 & how to avoid
- Mistake 2 & how to avoid

## See Also
- Link to related doc
- Link to tool reference
```

### Why This Matters
- AI can quickly understand document purpose
- Metadata helps semantic search prioritize relevant docs
- "When to Use" prevents misapplication
- "See Also" creates context links

---

## 4. Explicit Examples Over Explanation

### Principle
**Show, don't tell. AI learns from patterns.**

### Anti-Pattern ❌
```markdown
# Configuration Best Practices

Configuration should be centralized and environment-aware. 
Use dependency injection to make configuration available 
throughout your application...
```

### Best Practice ✅
```markdown
# Configuration Best Practices

## Example: Correct Configuration

```csharp
public class ApplicationSettings
{
    public string ConnectionString { get; set; }  // Required!
    public int TimeoutSeconds { get; set; }       // Default: 30
    public bool LoggingEnabled { get; set; }      // Default: true
}
```

**Why this works:**
- ✅ Explicit properties
- ✅ No fallback logic
- ✅ Clear defaults in comments

## Example: INCORRECT (What NOT to do)

```csharp
// ❌ DON'T: Uses fallback logic
var timeout = settings.TimeoutSeconds ?? 30;
```

**Why this is wrong:**
- Fallback hides missing configuration
- Will silently use default if not set
- Bug doesn't surface until production
```

### Why This Matters
- AI learns patterns from examples
- Actual code is less ambiguous than description
- Shows both "right" and "wrong" for comparison
- Gives AI concrete patterns to replicate

---

## 5. Avoid Speculation & Conjecture

### Principle
**Only document what you know. Only show what you've tested.**

### Anti-Pattern ❌
```markdown
# Potential Performance Improvements

These patterns *might* improve performance:
- Consider using caching (untested)
- You could try async/await (in some cases)
- Maybe use connection pooling (theory)
```

### Best Practice ✅
```markdown
# Proven Performance Patterns

## Pattern: Connection Pooling

**Measurement:**
- Before: 500ms average query time
- After: 150ms average query time
- Impact: 70% improvement

**Code:**
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
);
```

**When to apply:**
- Multiple concurrent database queries
- Connection creation is bottleneck
```

### Why This Matters
- Speculation creates confusion
- AI may amplify uncertainties
- Tested patterns are reliable
- Measurements provide trust

---

## 6. Link Structure & Context

### Principle
**Create semantic relationships, not hierarchies.**

### Anti-Pattern ❌
```markdown
# Folder Structure
- /patterns
  - /api
  - /database
  - /security

# In a document:
"See the database folder for more"
```

### Best Practice ✅
```markdown
# Your Document

See also:
- **Related Pattern**: [Database Query Optimization](../database/query-optimization.md)
- **Related Tool**: [DbContext Validator](../tools/dbcontext-validator.md)
- **Common Mistake**: [N+1 Query Problem](../gotchas/n-plus-one-queries.md)

---

# In another document

See also:
- **Prerequisite**: [Your Document](./your-doc.md)
```

### Why This Matters
- Bidirectional links help AI understand relationships
- "Related Pattern" signals when to reference
- Reduces "lost in forest" feeling
- Creates web of context rather than tree

---

## 7. Explicit "When NOT to Use"

### Principle
**Anti-patterns are as important as patterns.**

### Template
```markdown
# [Pattern Name]

## When to Use
- Scenario A
- Scenario B

## When NOT to Use ❌
- Anti-pattern 1: Why it fails
- Anti-pattern 2: Why it fails

## Common Mistakes
- Mistake 1: The error, the fix
- Mistake 2: The error, the fix

## Real-World Example
- Here's where this went wrong
- Here's how we fixed it
```

### Why This Matters
- AI often generates patterns without constraints
- Explicit "don't do this" prevents bad suggestions
- Anti-patterns show edge cases
- Real failures create pattern boundaries

---

## 8. Version & Date Everything

### Principle
**Knowledge becomes stale. Track when.**

### Best Practice
```markdown
# [Topic]

**Last Updated:** 2025-11-13  
**Applies To:** .NET 9, C# 13  
**Status:** Stable / Experimental / Deprecated  
**Supersedes:** [Link if applicable]  
```

### Why This Matters
- AI may reference outdated information
- Dating helps identify old vs. new patterns
- Version info prevents wrong tech stack application
- Deprecation notices prevent legacy patterns

---

## 9. Explicit Configuration Over Convention

### Principle
**Never rely on defaults. Document everything.**

### Anti-Pattern ❌
```markdown
# Configuration

Add this to `Program.cs`:

```csharp
builder.AddAuthentication();
```

By default, this uses JWT with a 15-minute expiration...
```

### Best Practice ✅
```markdown
# Configuration

Add this to `Program.cs`:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.JwtSettings.Issuer,
            ValidAudience = settings.JwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(settings.JwtSettings.SecretKey)
            )
        };
    });
```

**This explicitly configures:**
- ✅ Issuer validation (not assumed)
- ✅ Audience validation (not assumed)
- ✅ Lifetime validation (not assumed)
- ✅ All values from settings (no hardcoded defaults)
```

### Why This Matters
- Conventions hide assumptions
- AI may not know all defaults
- Explicit code is reproducible
- Prevents "works in dev, fails in prod"

---

## 10. Task-Based Organization (Not Feature-Based)

### Principle
**Organize by "how to do X" not "what is X".**

### Anti-Pattern ❌
```
/patterns
  /entity-framework
  /authentication
  /database
```

### Best Practice ✅
```
/common-tasks
  /add-new-entity.md
  /add-new-api-endpoint.md
  /implement-authentication.md
  /optimize-slow-query.md
  /handle-errors.md
```

### Why This Matters
- Users (and AI) think in tasks
- Task-based organization maps to real workflows
- "How do I..." finds answers faster
- Reduces cognitive load

---

## 11. Avoid Nested Dependencies

### Principle
**Each document should work independently.**

### Anti-Pattern ❌
```markdown
# Advanced Patterns

To understand this, you must first read:
1. Basic Patterns (5000 words)
2. Entity Framework Deep Dive (8000 words)
3. Performance Optimization Basics (6000 words)
```

### Best Practice ✅
```markdown
# Advanced Caching Pattern

**Prerequisite Knowledge:**
- [Basic Caching Pattern](../basics/caching.md) (quick reference)
- How Redis works (quick 1-para explanation if needed)

**This Document Assumes:**
- You know how to configure services in Program.cs
- You're using Entity Framework Core

**If you're unfamiliar with these, see:**
- [Program.cs Configuration](../setup/program-cs.md)
- [Entity Framework Basics](../database/ef-basics.md)
```

### Why This Matters
- AI context windows are finite
- Reading 20k words to understand 1k-word pattern is wasteful
- Independent documents are more reusable
- Reduces friction for newcomers

---

## 12. Measurements & Data Over Theory

### Principle
**Back up claims with evidence.**

### Anti-Pattern ❌
```markdown
# Async/Await Performance

Async/await improves performance significantly...
For most applications, this is the recommended approach...
```

### Best Practice ✅
```markdown
# Async/Await Performance

**Measurement (Real Data):**
- Sync method: 450ms average, blocks thread
- Async method: 400ms average, frees thread for other work
- Under 100 concurrent requests: no measurable difference
- Under 1000 concurrent requests: async shows 35% throughput improvement

**Why:**
- Sync: Each request holds thread until complete
- Async: Thread returns to pool, serves other requests
- **When to optimize**: High concurrency scenarios (>500 req/sec)
- **Not recommended for**: Single-threaded apps or low-concurrency APIs

**Real-World Context:**
- E-commerce platform: 50,000 req/day (async recommended)
- Internal reporting tool: 10 req/day (sync is fine)
```

### Why This Matters
- Measurements prevent over-engineering
- Data shows when optimization matters
- AI learns realistic constraints
- Builds trust in recommendations

---

## 13. State & Mutation Management

### Principle
**Document how state changes. Don't assume immutability.**

### Example Template
```markdown
# [Feature] State Management

## Initial State
- Property A: Requires non-null value
- Property B: Optional, defaults to null
- Property C: Array, defaults to empty

## State Transitions
1. **Initialization** → Set by [service]
2. **Validation** → Checked by [validator]
3. **Update** → Modified by [update method]
4. **Cleanup** → Cleared by [cleanup task]

## Invariants (Always True)
- Property A is never null
- Property B remains null until explicitly set
- Property C length never exceeds 1000

## Mutation Points
- X mutated here (with audit trail)
- Y mutated here (with validation)

## Never Mutated
- Z is immutable (read-only)
```

### Why This Matters
- Concurrent systems have state bugs
- AI may not track mutable state
- Explicit rules prevent race conditions
- Invariants catch bugs

---

## 14. Error Handling & Failure Modes

### Principle
**Document not just success, but failures.**

### Template
```markdown
# [Operation] Error Handling

## Success Path
[Normal case]

## Failure Paths

### Failure 1: Database Connection Lost
- **Error**: `SqlException`
- **When**: Database unavailable
- **Handling**: Retry with exponential backoff
- **User Message**: "Temporarily unavailable, retrying..."

### Failure 2: Invalid Input
- **Error**: `ValidationException`
- **When**: User provides bad data
- **Handling**: Return detailed validation errors
- **User Message**: "Please fix these fields: [list]"

### Failure 3: Permission Denied
- **Error**: `UnauthorizedException`
- **When**: User lacks required role
- **Handling**: Log attempt, return 403
- **User Message**: "You don't have permission"
```

### Why This Matters
- Happy path code is incomplete
- Failure modes reveal system design
- AI should generate error handling
- Prevents silent failures

---

## 15. The "Why" Behind Rules

### Principle
**Never state a rule without explaining it.**

### Anti-Pattern ❌
```markdown
# Rules

1. Never use ?? operator
2. Always validate input
3. Never trust user data
```

### Best Practice ✅
```markdown
# Rules

### Rule 1: Never Use ?? (Fallback) Operator

**The Rule:**
```csharp
❌ var timeout = settings.Timeout ?? 30;
✅ var timeout = settings.Timeout;  // Throw if null
```

**Why:**
- Fallback logic hides configuration errors
- Silent defaults mask bugs until production
- If timeout is required, missing it should fail loudly at startup
- Allows catching errors during testing, not after deployment

**Real Example:**
- Last year: Missing JWT secret used fallback value → silent auth bypass
- Fixed by: Removing fallback → startup fails immediately if secret missing
- Lesson: Fail fast is safer than silent defaults

**When This Rule Doesn't Apply:**
- Truly optional values (e.g., user notes field)
- Backwards compatibility (only when necessary)
```

### Why This Matters
- Rules without reasoning are arbitrary
- Understanding "why" helps AI apply rules correctly
- Context prevents bad generalizations
- Developers trust documented reasoning

---

## Summary: Core Principles

| Principle | Implementation |
|-----------|-----------------|
| **Semantic Organization** | Group by problem domain, not structure |
| **Small Documents** | 500-2000 words each |
| **Self-Describing** | Headers, metadata, purpose statements |
| **Show Don't Tell** | Real code examples |
| **No Speculation** | Only tested patterns |
| **Link Everything** | Bidirectional references |
| **Anti-Patterns** | Document what NOT to do |
| **Versioned** | Date, version, status on everything |
| **Explicit** | No hidden defaults or conventions |
| **Task-Based** | Organize by workflows |
| **Independent** | Each doc works alone |
| **Data-Driven** | Measurements > Theory |
| **State-Aware** | Document mutations and invariants |
| **Failure-First** | Error paths as important as happy paths |
| **Explain Why** | Rules without reasoning are arbitrary |

---

## Implementation Checklist for Your KB

- [ ] Organize by problem domain (not file structure)
- [ ] Keep documents under 2000 words
- [ ] Add metadata header to each document
- [ ] Include "When NOT to use" section
- [ ] Provide 2+ real code examples
- [ ] Date all documents
- [ ] Create bidirectional links
- [ ] Document failure modes
- [ ] Explain the "why" behind rules
- [ ] Remove speculation (only tested patterns)
- [ ] Add measurements where relevant
- [ ] Create task-based index
- [ ] Include anti-patterns
- [ ] Make documents independent

---

**Status:** Complete best practices framework  
**Next Step:** Apply these principles to reorganize your KB

