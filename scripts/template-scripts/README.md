# WebTemplate Template - Project Generator

This directory contains scripts to generate new projects from the WebTemplate template.

## Quick Start

```powershell
# Interactive mode (recommended for first use)
.\New-ProjectFromTemplate.ps1

# Non-interactive mode
.\New-ProjectFromTemplate.ps1 -NonInteractive -ProjectName "MyNewProject" -TargetPath "C:\Projects\MyNewProject"

# Skip git initialization
.\New-ProjectFromTemplate.ps1 -SkipGit

# Skip validation
.\New-ProjectFromTemplate.ps1 -SkipValidation
```

## What It Does

The script performs a complete A-Z project setup:

### 1. **Collects Information** ✓
- Project name (with validation)
- Target directory (with validation)
- Database name
- Connection strings

### 2. **Validates Template** ✓
- Ensures all required directories exist
- Verifies template structure integrity

### 3. **Copies Files** ✓
- Uses robocopy for efficient copying
- Excludes: `.git`, `bin`, `obj`, `node_modules`, `template-scripts`
- Preserves directory structure

### 4. **Rebrands Project** ✓
- Renames all files and directories containing "WebTemplate"
- Updates namespaces in all code files
- Updates project references in `.csproj` and `.sln` files
- Handles all variations: WebTemplate, WebTemplate, webtemplate-frontend, WebTemplateDb

### 5. **Updates Configurations** ✓
- **appsettings.json**: Connection strings, JWT settings, admin email
- **package.json**: Project name and description
- **copilot-instructions.md**: Project-specific instructions
- **README files**: Adds creation timestamp

### 6. **Initializes Git** ✓
- Creates fresh git repository
- Generates comprehensive `.gitignore`
- Makes initial commit

### 7. **Validates Result** ✓
- Checks directory structure
- Validates solution file
- Attempts build (if dependencies restored)
- Verifies configuration files

## Module Structure

```
template-scripts/
├── New-ProjectFromTemplate.ps1    # Main orchestrator script
├── modules/
│   ├── UI-Helpers.ps1             # User interface functions
│   ├── File-Operations.ps1        # File system operations
│   ├── Rebranding.ps1             # Namespace and name replacements
│   ├── Configuration.ps1          # Config file updates
│   ├── Git-Operations.ps1         # Git initialization
│   └── Validation.ps1             # Project validation
└── README.md                      # This file
```

### Module Responsibilities

#### **UI-Helpers.ps1**
- Banner display
- Progress reporting
- User input collection with validation
- Completion summary

#### **File-Operations.ps1**
- Project information gathering
- Template structure validation
- File copying with exclusions
- File enumeration for rebranding

#### **Rebranding.ps1**
- Directory and file renaming
- Content search and replace
- Namespace updates
- Case-sensitive replacements

#### **Configuration.ps1**
- appsettings.json updates
- package.json updates
- Copilot instructions updates
- README file modifications

#### **Git-Operations.ps1**
- Git repository initialization
- .gitignore generation
- Initial commit creation

#### **Validation.ps1**
- Structure validation
- Solution file checking
- Build testing
- Configuration verification

## Parameters

### `-NonInteractive`
Skip all prompts and use provided or default values.

```powershell
.\New-ProjectFromTemplate.ps1 -NonInteractive -ProjectName "MyProject"
```

### `-ProjectName`
Specify the new project name (required in non-interactive mode).

```powershell
.\New-ProjectFromTemplate.ps1 -ProjectName "CustomerPortal"
```

### `-TargetPath`
Specify where to create the project (optional, defaults to parent directory).

```powershell
.\New-ProjectFromTemplate.ps1 -TargetPath "C:\Projects\MyNewProject"
```

### `-SkipGit`
Don't initialize git repository.

```powershell
.\New-ProjectFromTemplate.ps1 -SkipGit
```

### `-SkipValidation`
Skip the final validation step.

```powershell
.\New-ProjectFromTemplate.ps1 -SkipValidation
```

## Example Workflow

### Interactive (Recommended)

```powershell
PS> .\New-ProjectFromTemplate.ps1

╔═══════════════════════════════════════════════════════════════════════════╗
║                                                                           ║
║               WebTemplate Template - New Project Generator                    ║
║                                                                           ║
╚═══════════════════════════════════════════════════════════════════════════╝

═══════════════════════════════════════════════════════════════════════════
  Step 1/7: Collecting Project Information
═══════════════════════════════════════════════════════════════════════════

Please provide information for your new project:

Project Name: CustomerPortal
Target Directory [C:\Projects\CustomerPortal]:
Database Name [CustomerPortalDb]:

═══════════════════════════════════════════════════════════════════════════
  Step 2/7: Validating Template
═══════════════════════════════════════════════════════════════════════════

ℹ Validating template structure...
✓ Template structure validated

...

╔═══════════════════════════════════════════════════════════════════════════╗
║                                                                           ║
║                     ✓ PROJECT CREATED SUCCESSFULLY                        ║
║                                                                           ║
╚═══════════════════════════════════════════════════════════════════════════╝
```

### Automated

```powershell
# CI/CD or batch creation
.\New-ProjectFromTemplate.ps1 `
    -NonInteractive `
    -ProjectName "EmployeeDashboard" `
    -TargetPath "C:\Projects\EmployeeDashboard"
```

## What Gets Rebranded

### Namespaces
- `WebTemplate.API` → `YourProject.API`
- `WebTemplate.Core` → `YourProject.Core`
- `WebTemplate.Data` → `YourProject.Data`
- `WebTemplate.UserModule` → `YourProject.UserModule`

### Project Files
- `WebTemplate.API.csproj` → `YourProject.API.csproj`
- `WebTemplate.sln` → `YourProject.sln`
- `webtemplate-frontend` → `yourproject-frontend`

### Configuration Values
- Database: `CoreWebTemplateDb` → `YourProjectDb`
- JWT Issuer: `CoreWebApp.API` → `YourProject.API`
- Admin Email: `admin@WebTemplate.com` → `admin@yourproject.com`

### File Contents
All occurrences in:
- `.cs` files
- `.csproj` files
- `.sln` files
- `.json` files
- `.md` files
- `.ps1` files
- `.ts` and `.tsx` files

## After Running the Script

Your new project is **100% ready** to build and run:

1. **Navigate to project**:
   ```bash
   cd C:\Projects\YourNewProject
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   npm install --prefix Frontend/yourproject-frontend
   ```

3. **Initialize database**:
   - Run: `Backend/WebTemplate.Data/Migrations/db-init.sql` in SQL Server

4. **Build**:
   ```bash
   dotnet build
   ```

5. **Run**:
   ```bash
   dotnet run --project Backend/YourProject.API/YourProject.API.csproj
   ```

6. **Validate**:
   ```bash
   pwsh scripts/validate-dbcontext.ps1
   ```

## Troubleshooting

### "Project file does not exist"
- Ensure you're running from the template workspace root
- Check that the template structure is intact

### "Directory already exists"
- Choose a different target path
- Delete the existing directory if it's a failed attempt

### Build fails after creation
- Run `dotnet restore` first
- Check that all file rebranding completed successfully
- Verify no "WebTemplate" references remain in code files

### Git initialization fails
- Ensure git is installed and in PATH
- Run manually: `cd TargetPath && git init`

## Adding Custom Steps

To add custom processing:

1. Create a new module in `modules/`
2. Define exported functions
3. Import in `New-ProjectFromTemplate.ps1`:
   ```powershell
   . (Join-Path $ModulesPath "Your-Module.ps1")
   ```
4. Call your function in the appropriate step

## Maintenance

### Updating Exclusions
Edit `File-Operations.ps1` → `Get-ProjectInformation` → `$exclusions` array

### Modifying Replacements
Edit `Rebranding.ps1` → `Update-FileContents` → `$replacements` hashtable

### Adding Validations
Edit `Validation.ps1` → Add new `Test-*` functions

## Best Practices

✅ **Always review** the generated project before committing
✅ **Test build** immediately after creation
✅ **Update README** with project-specific information
✅ **Configure secrets** (JWT keys, connection strings) for production
✅ **Customize** copilot-instructions.md for your project

## Exit Codes

- `0` = Success
- `1` = Failure (with error message)

## Notes

- This script **does not** copy itself to the new project
- Template workspace remains untouched
- Generated projects are completely independent
- Safe to run multiple times (with different target paths)

---

**Ready to create your first project?**

```powershell
.\New-ProjectFromTemplate.ps1
```
