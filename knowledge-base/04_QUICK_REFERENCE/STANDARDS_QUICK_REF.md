# Quick Reference: Standards

**Last Updated:** 13/11/2025  
**Purpose:** One-page lookup for critical standards

---

## NO FALLBACK LOGIC (Critical)

```csharp
❌ WRONG:
var value = config["key"] ?? defaultValue;
var timeout = options.Timeout.GetValueOrDefault();

✅ RIGHT:
var value = config["key"] 
  ?? throw new ConfigurationException("'key' required");
var timeout = options.Timeout 
  ?? throw new ArgumentException("Timeout must be set");
```

---

## SQL-FIRST Approach

1. **db-init.sql** is authoritative
2. Entities follow schema (not reverse)
3. DbContext validator ensures match
4. No EF migrations for schema

---

## Explicit Validation

```csharp
if (required_field == null)
    throw new ArgumentException("required_field is required");
    
// Don't proceed with uncertain state
```

---

## Fail Fast

- Validate at entry
- Throw on invalid state
- Don't continue with partial data
- Surface errors immediately

---

## Greenfield Mindset

- ✅ Breaking changes OK
- ✅ Refactor freely
- ✅ Improve architecture
- ❌ NO backward compat needed

---

**For details, see 01_CORE_PARTNERSHIP/02_YOUR_STANDARDS.md**
