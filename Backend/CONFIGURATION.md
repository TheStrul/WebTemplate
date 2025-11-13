# Configuration Pattern

Status: Draft (update as implementation evolves)

## Goal
Single, fail-fast, deterministic configuration source shared across layers (`API`, `Core`, `Data`, tests). No hidden defaults. No fallbacks. Any missing required value breaks startup immediately.

## Core Principles
- ONE CONFIGURATION TO RULE THEM ALL
- NO FALLBACKS (never use `??`, `||`, silent defaults for required values)
- Fail fast (constructor validation throws `InvalidOperationException`)
- Deterministic (same inputs -> same runtime state)
- Layered responsibility (validation lives in config types, not scattered)
- Explicit environments only: `Development | Staging | Production`

## Base Interface: `IBaseConfiguration`
Defined in `Backend/WebTemplate.Core/Configuration/IBaseConfiguration.cs`:
```csharp
public interface IBaseConfiguration
{
    // Performs validation; must throw on any invalid/missing required value.
    // Returns true if validation succeeds.
    bool Validate();
}
```
Purpose:
- Provides a uniform validation contract for all configuration roots.
- Constructors perform mandatory binding + validation; `Validate()` is an explicit hook (returns true after successful construction).
- Any implementation must throw on missing/invalid required values; never supply fallbacks.

## Core Interface: `ICoreConfiguration`
Defined in `Backend/WebTemplate.Core/Configuration/ICoreConfiguration.cs` and extends `IBaseConfiguration`.
Responsibilities:
- Declare ONLY the configuration objects directly required by the `WebTemplate.Core` project (domain/auth/email/urls/admin seed/features/messages).
- Acts as the contract consumed by Core services without exposing hosting concerns (`Server`, `Database`).
- Validation: The concrete `CoreConfiguration` implements `Validate()` and returns `true` after constructor-based validation. When extended (e.g. by `ApplicationConfiguration`), overrides should call `base.Validate()` first; if it returns `true`, perform additional validation then return `true` again. Pattern:
```csharp
public override bool Validate()
{
    if (!base.Validate()) return false; // (Current base always returns true; placeholder for future logic.)
    // Additional validation for derived configuration.
    return true;
}
```
This chaining model allows layered configuration types to build on prior validation without duplicating checks.

## Hierarchy
1. `CoreConfiguration` (implements `ICoreConfiguration : IBaseConfiguration`) – domain/auth/email/urls/admin seed/messages features.
2. `ApplicationConfiguration` (extends `CoreConfiguration`) – adds `Server` + `Database`.
3. `DataConfiguration` (implements `IDataConfiguration : ICoreConfiguration`) – adds connection string + retry settings.
4. Interfaces expose only what a layer needs (`ICoreConfiguration`, `IDataConfiguration`).

## Environment Loading Strategy
Development:
- Load ONLY `appsettings.Local.json` (contains all required values – no secrets store needed).

Staging / Production:
- Load layered files: `appsettings.json` then `appsettings.{Environment}.json` then environment variables.

Validation of `ASPNETCORE_ENVIRONMENT` occurs before building configuration. Disallowed or missing values terminate startup (`Environment.Exit(1)` in `Program`).

## Validation Flow
- Each configuration class binds a section then validates immediately.
- Required section missing => throw.
- Each property validated for non-empty / range / format.
- `Validate()` virtual/override returns `true` (all work done in constructor). Override only if additional post-construction checks are added; call `base.Validate()` first when chaining.

## Required Sections (must exist in effective configuration set)
- `JwtSettings`
- `AuthSettings`
- `Email`
- `AppUrls`
- `AdminSeed` (when `AdminSeed:Enabled=true` then all nested required values must exist)
- `Server` (in `ApplicationConfiguration`)
- `Database` (in `ApplicationConfiguration`)
- Connection string accessed via `ConnectionStrings:DefaultConnection` for `DataConfiguration`

Optional Sections (allow empty object with defaults)
- `UserModule:Features`
- `ResponseMessages`

## Local vs Non-Local Behavior
| Aspect | Development | Staging/Production |
|--------|-------------|--------------------|
| Source files | `appsettings.Local.json` only | `appsettings.json` + `appsettings.{Env}.json` |
| Env vars | Ignored | Merged (override) |
| User secrets | Not used | Not used (policy: explicit files / env vars) |
| Missing required value | Startup abort | Startup abort |

## Adding a New Required Setting
1. Define POCO with `SectionName` constant in `WebTemplate.Core/Configuration`.
2. Bind + validate in correct configuration class constructor (throw on missing/invalid).
3. Add section to `appsettings.Local.json`, `appsettings.json`, and environment-specific file(s).
4. Update this document: list section in Required Sections.
5. If exposed to other layers, add property to interface (`ICoreConfiguration` or derived).
6. Run build; ensure failure occurs if section removed.

## Adding Optional Feature Flag Group
1. Define POCO.
2. Bind with `Get<T>() ?? new T()` (only for truly optional group). Avoid fallback for individual required properties.
3. Document in Optional Sections.

## Extension Checklist
- [ ] New section added to all config files
- [ ] Constructor binding added
- [ ] Validation function updated / created
- [ ] Interface updated (if needed)
- [ ] Tests adjusted
- [ ] This file updated

## Fail-Fast Patterns
Good:
```csharp
var settings = section.Get<MySettings>() ?? throw new InvalidOperationException("MySettings section missing.");
if (string.IsNullOrWhiteSpace(settings.RequiredValue))
    throw new InvalidOperationException("MySettings:RequiredValue missing.");
```
Bad (DO NOT):
```csharp
var settings = section.Get<MySettings>() ?? new MySettings(); // Fallback hides bug
settings.RequiredValue ??= "default"; // Silent default
```

## Azure / Hosting Considerations (Planned)
Potential explicit marker (e.g. `AZURE_DEPLOYMENT=1`) for non-development validation:
- If `Environment != Development` require marker + platform variable (e.g. `WEBSITE_SITE_NAME`).
- Forbid marker in Development.
(Implement when deployment platform fixed; update doc.)

## Testing Guidelines
- Integration / API tests override config with in-memory collection for essentials only.
- E2E tests instantiate `ApplicationConfiguration` directly to keep behavior identical.
- Tests must never introduce defaults that mask missing required config.

## Future Enhancements
- Central diagnostic dump method (redact secrets) on validation pass.
- Structured logging of configuration summary at startup.
- Build-time schema check comparing config file keys vs bound POCOs.

## Update Log
- v0.1 Initial draft documenting existing pattern (Core + Application configuration, virtual `Validate()`).
- v0.2 Added Base Interface section documenting `IBaseConfiguration`.
- v0.3 Added `ICoreConfiguration` responsibilities and Validate chaining pattern.

---
Maintain this file as the authoritative description of configuration architecture. Every change to configuration code requires an update here.

