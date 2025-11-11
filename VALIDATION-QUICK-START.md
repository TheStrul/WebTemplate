# DbContext Validator - Quick Reference

## Run Validation

```powershell
# From project root
pwsh -File scripts\validate-dbcontext.ps1
```

## What Gets Validated

✅ **All entities in `WebTemplate.Core.Entities`** are registered in `ApplicationDbContext`
✅ **`[Required]` attributes** match Fluent API `IsRequired()`
✅ **`[StringLength(N)]` attributes** match `HasMaxLength(N)`
✅ **`[Key]` attributes** match primary key configuration
✅ **Foreign keys** have proper relationships configured
✅ **Indexes** exist where expected

## Exit Codes

- `0` = ✓ PASS - All configurations match
- `1` = ✗ FAIL - Configuration issues found
- `2` = ✗ ERROR - Tool execution failed

## Example Output

### Success
```
Entities Validated: 3
Total Issues: 0
Status: ✓ PASS
✓ All entity configurations match 100% with DbContext!
```

### Failure
```
Entities Validated: 3
Total Issues: 2
Status: ✗ FAIL

ERRORS (2):
[ERROR] ApplicationUser.FirstName: Property 'FirstName' StringLength mismatch
    Expected: MaxLength=100
    Actual: MaxLength=50
```

## When to Run

- ✅ Before committing entity changes
- ✅ In CI/CD pipeline
- ✅ After modifying `ApplicationDbContext`
- ✅ When adding new entities

## Integration

### CI/CD (GitHub Actions)
```yaml
- name: Validate DbContext
  run: pwsh -File scripts/validate-dbcontext.ps1
```

### Pre-commit Hook
```bash
#!/bin/bash
pwsh -File scripts/validate-dbcontext.ps1 || exit 1
```

## Files

- `Backend/WebTemplate.Data/Validation/DbContextValidator.cs` - Core validator
- `Backend/WebTemplate.Data.Validator/` - Executable tool
- `scripts/validate-dbcontext.ps1` - PowerShell runner

## No Database Required

✅ Validates **in-memory** using EF Core model metadata
✅ No migrations needed
✅ No database connection required
✅ Fast execution (~2-3 seconds)
