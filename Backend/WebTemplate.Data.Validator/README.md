# DbContext Validation Tool

## Purpose
Ensures 100% match between entity definitions in `WebTemplate.Core.Entities` and the configuration in `ApplicationDbContext`. This prevents configuration drift and catches missing entity registrations.

## What It Validates

### ✅ Entity Registration
- All entities in `WebTemplate.Core.Entities` namespace are registered in `ApplicationDbContext`
- No entities are missing from DbContext configuration

### ✅ Property Configuration
- **`[Required]` attributes** match `IsRequired()` in Fluent API
- **`[StringLength]` attributes** match `HasMaxLength()` configuration
- **`[Key]` attributes** match primary key configuration
- All non-computed properties are properly mapped

### ✅ Relationships & Foreign Keys
- Navigation properties with `[ForeignKey]` attributes have matching FK configuration
- Delete behaviors are properly configured (Cascade/Restrict)
- Relationship cardinality is correct

### ✅ Indexes
- Unique indexes exist where needed
- Performance indexes on foreign keys
- Composite indexes are properly configured

## How to Run

### Option 1: PowerShell Script (Recommended)
```powershell
# From project root
.\scripts\validate-dbcontext.ps1
```

### Option 2: Direct Execution
```bash
# From project root
cd Backend\WebTemplate.Data.Validator
dotnet run
```

### Option 3: As Part of Build
Add to your CI/CD pipeline:
```yaml
- name: Validate DbContext
  run: dotnet run --project Backend/WebTemplate.Data.Validator/WebTemplate.Data.Validator.csproj
```

## Exit Codes
- **0** = All validations passed ✓
- **1** = Validation issues found ✗
- **2** = Fatal error (tool execution failed)

## Sample Output

### ✓ Successful Validation
```
================================================================================
DbContext Validation Report
================================================================================
Entities Validated: 3
Total Issues: 0
Status: ✓ PASS

✓ All entity configurations match 100% with DbContext!
```

### ✗ Failed Validation
```
================================================================================
DbContext Validation Report
================================================================================
Entities Validated: 3
Total Issues: 2
Status: ✗ FAIL

ERRORS (2):
--------------------------------------------------------------------------------
[ERROR] ApplicationUser.FirstName: Property 'FirstName' StringLength mismatch
    Expected: MaxLength=100
    Actual: MaxLength=50

[ERROR] RefreshToken: Entity 'RefreshToken' is not registered in ApplicationDbContext

WARNINGS (1):
--------------------------------------------------------------------------------
[WARN] ApplicationUser.UserTypeId: Foreign key 'UserTypeId' might benefit from an index
```

## When to Run This Tool

### Required
- **Before committing** entity or DbContext changes
- **In CI/CD pipeline** to catch configuration drift
- **After adding new entities** to verify registration
- **After modifying entity properties** to ensure sync

### Recommended
- **Daily during active development** as a sanity check
- **Before deployments** to production
- **During code reviews** of data layer changes

## Integration with Development Workflow

### Pre-Commit Hook
Add to `.git/hooks/pre-commit`:
```bash
#!/bin/bash
dotnet run --project Backend/WebTemplate.Data.Validator/WebTemplate.Data.Validator.csproj
if [ $? -ne 0 ]; then
    echo "❌ DbContext validation failed - commit blocked"
    exit 1
fi
```

### VS Code Task
Add to `.vscode/tasks.json`:
```json
{
  "label": "Validate DbContext",
  "type": "shell",
  "command": "dotnet",
  "args": [
    "run",
    "--project",
    "Backend/WebTemplate.Data.Validator/WebTemplate.Data.Validator.csproj"
  ],
  "problemMatcher": [],
  "group": "test"
}
```

## Configuration

The tool reads connection strings from:
1. `appsettings.json` (copied from WebTemplate.API)
2. `appsettings.Development.json` (copied from WebTemplate.API)
3. Environment variables

No special configuration needed - it uses the same settings as your API project.

## Troubleshooting

### "No connection string found"
- Ensure `appsettings.json` exists in `Backend/WebTemplate.API/`
- Check that `ConnectionStrings:DefaultConnection` is defined

### "Could not find entity type"
- Entity might not be in `WebTemplate.Core.Entities` namespace
- Entity might be abstract or compiler-generated (these are skipped)
- Identity base classes (like `IdentityUser`) are automatically excluded

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet build Backend/WebTemplate.Data.Validator/WebTemplate.Data.Validator.csproj
```

## Architecture

### Project Structure
```
Backend/
├── WebTemplate.Data/
│   └── Validation/
│       ├── DbContextValidator.cs       # Core validation logic
│       └── ValidateDbContextTool.cs    # CLI entry point
└── WebTemplate.Data.Validator/
    ├── Program.cs                      # Main executable
    ├── WebTemplate.Data.Validator.csproj    # Tool project
    └── appsettings.json (linked)       # Configuration
```

### How It Works
1. **Discovery**: Reflects over `WebTemplate.Core.Entities` to find all entity types
2. **Inspection**: Uses EF Core metadata API to read `ApplicationDbContext` configuration
3. **Comparison**: Matches Data Annotations against Fluent API configuration
4. **Reporting**: Generates detailed report with all mismatches

### Validation Rules

#### Property Validation
- `[Required]` → Must have `IsRequired()` or non-nullable type
- `[StringLength(N)]` → Must have `HasMaxLength(N)`
- `[Key]` → Must be configured as primary key
- `[NotMapped]` → Should not appear in EF metadata

#### Relationship Validation
- `[ForeignKey("PropName")]` → Must have matching FK in EF metadata
- Navigation properties → Must have proper cardinality configured
- Delete behaviors → Must be explicitly set (Cascade/Restrict/SetNull)

#### Index Validation
- Unique properties → Should have unique index
- Foreign keys → Should have performance index
- Commonly queried fields → Warning if no index

## Extending the Validator

### Adding Custom Rules
Edit `Backend/WebTemplate.Data/Validation/DbContextValidator.cs`:

```csharp
private void ValidateCustomRule(IEntityType efEntityType, Type entityType)
{
    // Your validation logic
    if (conditionFails)
    {
        _issues.Add(new ValidationIssue
        {
            Severity = IssueSeverity.Error,
            EntityName = entityType.Name,
            Message = "Your custom message"
        });
    }
}
```

Call it from `ValidateEntity()` method.

## Benefits

✅ **Prevents Runtime Errors**: Catch configuration mismatches at build time
✅ **Maintains Consistency**: Ensures single source of truth between attributes and Fluent API
✅ **Documentation**: Serves as living documentation of required configurations
✅ **CI/CD Safety**: Blocks deployments with invalid configurations
✅ **Refactoring Confidence**: Verify nothing broke after schema changes

## No Migration Dependency

This tool validates configuration **without requiring migrations**. It works by:
- Reading entity class definitions (Data Annotations)
- Reading DbContext model metadata (Fluent API)
- Comparing them in-memory

**No database connection required** - it only needs to build the EF Core model.

---

**Note**: This tool is part of the greenfield architecture - no backward compatibility constraints! Feel free to extend with additional validation rules as needed.
