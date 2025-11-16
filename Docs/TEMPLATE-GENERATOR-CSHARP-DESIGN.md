# Template Generator C# Design Document

## Executive Summary

This document provides a comprehensive design for replacing the PowerShell-based template generation script with a robust, type-safe, modular C# implementation. The new design eliminates PowerShell dependencies, provides compile-time safety, better error handling, and full integration with the existing WebTemplate.Setup WinForms application.

## Current Implementation Analysis

### PowerShell Script Structure (Current)

**Main Script:** `scripts/template-scripts/New-ProjectFromTemplate.ps1`

**Modules:**
1. **File-Operations.ps1** - Template copying, validation, file filtering
2. **Rebranding.ps1** - File/folder renaming, content replacement
3. **Configuration.ps1** - JSON updates (appsettings, package.json, copilot-instructions)
4. **Git-Operations.ps1** - Git initialization, GitHub setup
5. **Validation.ps1** - Post-generation validation
6. **UI-Helpers.ps1** - Console output formatting

**Workflow (7 Steps):**
1. Collect Project Information (name, path, database)
2. Validate Template Structure
3. Copy Template Files (with include/exclude filters)
4. Rebrand Project (rename files/folders, update content)
5. Update Configurations (JSON files)
6. Initialize Git Repository
7. Validate New Project

### Problems with Current Implementation

1. **No Type Safety** - Runtime errors for typos, parameter mismatches
2. **Complex String Handling** - Quote escaping issues, path parsing problems
3. **Limited Debugging** - PowerShell stack traces are hard to interpret
4. **Zero IntelliSense** - No IDE support for PowerShell development
5. **External Dependency** - Requires `pwsh` to be installed
6. **Fragile Error Handling** - Variable reference issues (e.g., `$folder:` parsed as drive)
7. **Hard to Maintain** - Complex module loading, hashtable-based data passing

## Proposed C# Architecture

### Project Structure

```
WebTemplate.Setup/
├── Services/
│   ├── ProjectGenerationService.cs (orchestrator - EXISTING)
│   ├── DatabaseService.cs (EXISTING)
│   ├── TemplateEngine/
│   │   ├── ITemplateEngine.cs
│   │   ├── TemplateEngine.cs (main implementation)
│   │   ├── Steps/
│   │   │   ├── IGenerationStep.cs
│   │   │   ├── ValidateTemplateStep.cs
│   │   │   ├── CopyFilesStep.cs
│   │   │   ├── RebrandProjectStep.cs
│   │   │   ├── UpdateConfigurationsStep.cs
│   │   │   ├── InitializeGitStep.cs
│   │   │   └── ValidateProjectStep.cs
│   │   ├── FileCopier.cs
│   │   ├── FileRebrander.cs
│   │   ├── ConfigurationUpdater.cs
│   │   └── GitInitializer.cs
├── Models/
│   ├── WorkspaceConfiguration.cs (EXISTING)
│   ├── TemplateContext.cs (NEW)
│   └── GenerationResult.cs (NEW)
└── Utilities/
    ├── FileSystemHelper.cs (NEW)
    └── JsonHelper.cs (NEW)
```

### Core Interfaces

```csharp
namespace WebTemplate.Setup.Services.TemplateEngine;

/// <summary>
/// Defines a single step in the template generation process
/// </summary>
public interface IGenerationStep
{
    string StepName { get; }
    int StepNumber { get; }

    Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from executing a generation step
/// </summary>
public record StepResult(
    bool Success,
    string Message,
    Exception? Exception = null);

/// <summary>
/// Main template engine interface
/// </summary>
public interface ITemplateEngine
{
    Task<GenerationResult> GenerateAsync(
        WorkspaceConfiguration config,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}
```

### Core Models

```csharp
namespace WebTemplate.Setup.Models;

/// <summary>
/// Contains all context needed during template generation
/// </summary>
public class TemplateContext
{
    public required WorkspaceConfiguration Configuration { get; init; }
    public required string TemplatePath { get; init; }
    public required string TargetPath { get; init; }
    public required string OldProjectName { get; init; }
    public required string NewProjectName { get; init; }
    public required string DatabaseName { get; init; }
    public required string ConnectionString { get; init; }

    // Include/Exclude patterns
    public List<string> IncludeFolders { get; init; } = ["Backend", "Frontend"];
    public List<string> ExcludeDirectories { get; init; } =
        [".git", ".vs", "bin", "obj", "node_modules", "build", "dist", ".vscode", "TestResults"];
    public List<string> ExcludeFiles { get; init; } =
        ["*.user", "*.suo", "*.log", "package-lock.json"];

    // Execution state
    public Dictionary<string, object> State { get; } = new();
}

/// <summary>
/// Final result of template generation
/// </summary>
public class GenerationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? GeneratedPath { get; init; }
    public List<StepResult> StepResults { get; init; } = [];
    public TimeSpan Duration { get; init; }
    public Exception? Exception { get; init; }
}
```

## Implementation Plan

### Phase 1: Core Infrastructure (Foundation)

**Goal:** Create the template engine foundation and step infrastructure

**Tasks:**
1. Create `IGenerationStep` interface
2. Create `ITemplateEngine` interface
3. Create `TemplateContext` model
4. Create `GenerationResult` model
5. Create `StepResult` record
6. Create base `TemplateEngine` class with step orchestration
7. Add comprehensive logging

**Deliverable:** Working step pipeline with no-op steps

---

### Phase 2: File Operations (Step 3)

**Goal:** Replace File-Operations.ps1 with C# implementation

**Component: FileCopier.cs**

```csharp
public class FileCopier
{
    private readonly ILogger<FileCopier> _logger;

    public async Task<CopyResult> CopyTemplateAsync(
        string sourcePath,
        string targetPath,
        List<string> includeFolders,
        List<string> excludeDirs,
        List<string> excludeFiles,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // Implementation:
        // 1. Validate source path exists
        // 2. Create target directory
        // 3. For each included folder:
        //    a. Copy recursively
        //    b. Skip excluded directories
        //    c. Skip excluded file patterns
        // 4. Report progress
        // 5. Return detailed statistics
    }
}
```

**Component: CopyFilesStep.cs**

```csharp
public class CopyFilesStep : IGenerationStep
{
    public string StepName => "Copying Template Files";
    public int StepNumber => 3;

    private readonly FileCopier _copier;

    public async Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report($"Step {StepNumber}/7: {StepName}");

        var result = await _copier.CopyTemplateAsync(
            context.TemplatePath,
            context.TargetPath,
            context.IncludeFolders,
            context.ExcludeDirectories,
            context.ExcludeFiles,
            progress,
            cancellationToken);

        return new StepResult(
            result.Success,
            $"Copied {result.FileCount} files, {result.DirectoryCount} directories");
    }
}
```

**Testing Strategy:**
- Unit tests with temporary directories
- Test exclusion patterns
- Test large file counts
- Test cancellation handling

---

### Phase 3: Rebranding (Step 4)

**Goal:** Replace Rebranding.ps1 with C# implementation

**Component: FileRebrander.cs**

```csharp
public class FileRebrander
{
    private readonly ILogger<FileRebrander> _logger;

    // Binary file extensions to skip
    private static readonly HashSet<string> BinaryExtensions =
        [".dll", ".exe", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".pdf", ".zip"];

    public async Task<RebrandResult> RebrandAsync(
        string targetPath,
        string oldName,
        string newName,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Rename files and directories (deepest first)
        var renameResult = await RenameStructureAsync(
            targetPath, oldName, newName, progress, cancellationToken);

        // 2. Update file contents
        var contentResult = await UpdateFileContentsAsync(
            targetPath, oldName, newName, progress, cancellationToken);

        return new RebrandResult(renameResult, contentResult);
    }

    private async Task<RenameResult> RenameStructureAsync(...)
    {
        // Use iterative approach (while loop) to handle parent path changes
        // Sort by depth (deepest first) to avoid parent-before-child issues
        // Track renamed items to avoid duplicates
    }

    private async Task<ContentUpdateResult> UpdateFileContentsAsync(...)
    {
        // 1. Find all text files (exclude binary extensions)
        // 2. For each file:
        //    a. Read content
        //    b. Replace all instances of oldName with newName
        //    c. Write back if changes detected
        // 3. Use UTF-8 encoding consistently
    }
}
```

**Component: RebrandProjectStep.cs**

```csharp
public class RebrandProjectStep : IGenerationStep
{
    public string StepName => "Rebranding Project";
    public int StepNumber => 4;

    private readonly FileRebrander _rebrander;

    public async Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report($"Step {StepNumber}/7: {StepName}");

        var result = await _rebrander.RebrandAsync(
            context.TargetPath,
            context.OldProjectName,
            context.NewProjectName,
            progress,
            cancellationToken);

        return new StepResult(
            result.Success,
            $"Renamed {result.FilesRenamed} files, updated {result.FilesModified} file contents");
    }
}
```

**Key Considerations:**
- Handle file locks gracefully
- Use `FileShare.Read` when possible
- Retry logic for locked files
- Skip binary files for content replacement
- Preserve file attributes and timestamps

---

### Phase 4: Configuration Updates (Step 5)

**Goal:** Replace Configuration.ps1 with C# implementation

**Component: ConfigurationUpdater.cs**

```csharp
public class ConfigurationUpdater
{
    private readonly ILogger<ConfigurationUpdater> _logger;

    public async Task<UpdateResult> UpdateConfigurationsAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<FileUpdateResult>();

        // 1. Update appsettings.json files
        results.AddRange(await UpdateAppSettingsAsync(context, progress, cancellationToken));

        // 2. Update package.json files
        results.AddRange(await UpdatePackageJsonAsync(context, progress, cancellationToken));

        // 3. Update .github/copilot-instructions.md
        results.AddRange(await UpdateCopilotInstructionsAsync(context, progress, cancellationToken));

        // 4. Update README.md files
        results.AddRange(await UpdateReadmeFilesAsync(context, progress, cancellationToken));

        return new UpdateResult(results);
    }

    private async Task<List<FileUpdateResult>> UpdateAppSettingsAsync(...)
    {
        // 1. Find all appsettings*.json files
        // 2. For each file:
        //    a. Parse JSON using System.Text.Json
        //    b. Update ConnectionStrings.DefaultConnection
        //    c. Save with proper formatting (indented, UTF-8)
    }

    private async Task<List<FileUpdateResult>> UpdatePackageJsonAsync(...)
    {
        // 1. Find all package.json files
        // 2. For each file:
        //    a. Parse JSON
        //    b. Update "name" property
        //    c. Update "description" if needed
        //    d. Save with proper formatting
    }

    private async Task<List<FileUpdateResult>> UpdateCopilotInstructionsAsync(...)
    {
        // 1. Find .github/copilot-instructions.md
        // 2. Replace old project name with new
        // 3. Update any hardcoded paths
    }

    private async Task<List<FileUpdateResult>> UpdateReadmeFilesAsync(...)
    {
        // 1. Find all README.md files
        // 2. Add header with project name and creation date
        // 3. Update any references to old project name
    }
}
```

**Component: UpdateConfigurationsStep.cs**

```csharp
public class UpdateConfigurationsStep : IGenerationStep
{
    public string StepName => "Updating Configurations";
    public int StepNumber => 5;

    private readonly ConfigurationUpdater _updater;

    public async Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report($"Step {StepNumber}/7: {StepName}");

        var result = await _updater.UpdateConfigurationsAsync(
            context, progress, cancellationToken);

        return new StepResult(
            result.Success,
            $"Updated {result.FilesModified} configuration files");
    }
}
```

**JSON Handling:**
- Use `System.Text.Json` for consistency with .NET 9
- Use `JsonSerializerOptions` with `WriteIndented = true`
- Handle missing properties gracefully
- Validate JSON structure before/after updates

---

### Phase 5: Git Initialization (Step 6)

**Goal:** Replace Git-Operations.ps1 with C# implementation

**Component: GitInitializer.cs**

```csharp
public class GitInitializer
{
    private readonly ILogger<GitInitializer> _logger;

    public async Task<GitResult> InitializeRepositoryAsync(
        string targetPath,
        string projectName,
        bool createInitialCommit,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Initialize git repository (git init)
        var initResult = await RunGitCommandAsync(
            "init", targetPath, cancellationToken);

        if (!initResult.Success)
            return GitResult.Failed($"Git init failed: {initResult.Error}");

        // 2. Create initial commit (if requested)
        if (createInitialCommit)
        {
            await RunGitCommandAsync("add .", targetPath, cancellationToken);
            await RunGitCommandAsync(
                $"commit -m \"Initial commit from WebTemplate\"",
                targetPath,
                cancellationToken);
        }

        return GitResult.Success($"Git repository initialized");
    }

    private async Task<CommandResult> RunGitCommandAsync(
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        // Execute and capture output/errors
        // Return CommandResult with exit code, output, error
    }
}
```

**Component: InitializeGitStep.cs**

```csharp
public class InitializeGitStep : IGenerationStep
{
    public string StepName => "Initializing Git Repository";
    public int StepNumber => 6;

    private readonly GitInitializer _gitInitializer;

    public async Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        // Skip if not requested
        if (!context.Configuration.Project.InitializeGit)
        {
            progress?.Report($"Step {StepNumber}/7: Skipping Git Initialization");
            return new StepResult(true, "Git initialization skipped");
        }

        progress?.Report($"Step {StepNumber}/7: {StepName}");

        var result = await _gitInitializer.InitializeRepositoryAsync(
            context.TargetPath,
            context.NewProjectName,
            context.Configuration.Project.CreateInitialCommit,
            progress,
            cancellationToken);

        return new StepResult(result.Success, result.Message);
    }
}
```

**Git Command Handling:**
- Check if `git` is available before attempting
- Provide friendly error messages if git not found
- Support both Windows and cross-platform scenarios
- Don't fail generation if git fails (warn only)

---

### Phase 6: Validation Steps (Steps 2 & 7)

**Goal:** Template validation before and project validation after

**Component: ValidateTemplateStep.cs**

```csharp
public class ValidateTemplateStep : IGenerationStep
{
    public string StepName => "Validating Template";
    public int StepNumber => 2;

    private readonly string[] _requiredPaths =
    [
        "Backend\\WebTemplate.API",
        "Backend\\WebTemplate.Core",
        "Backend\\WebTemplate.Data",
        "Frontend\\WebTemplate-frontend"
    ];

    public Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report($"Step {StepNumber}/7: {StepName}");

        var missingPaths = new List<string>();

        foreach (var requiredPath in _requiredPaths)
        {
            var fullPath = Path.Combine(context.TemplatePath, requiredPath);
            if (!Directory.Exists(fullPath))
            {
                missingPaths.Add(requiredPath);
            }
        }

        if (missingPaths.Any())
        {
            return Task.FromResult(new StepResult(
                false,
                $"Template validation failed. Missing paths: {string.Join(", ", missingPaths)}"));
        }

        return Task.FromResult(new StepResult(true, "Template structure validated"));
    }
}
```

**Component: ValidateProjectStep.cs**

```csharp
public class ValidateProjectStep : IGenerationStep
{
    public string StepName => "Validating New Project";
    public int StepNumber => 7;

    public Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        // Skip if not requested
        if (!context.Configuration.Project.RunValidation)
        {
            progress?.Report($"Step {StepNumber}/7: Skipping Validation");
            return Task.FromResult(new StepResult(true, "Validation skipped"));
        }

        progress?.Report($"Step {StepNumber}/7: {StepName}");

        var issues = new List<string>();

        // 1. Check that Backend folder exists and renamed
        var backendPath = Path.Combine(
            context.TargetPath,
            "Backend",
            $"{context.NewProjectName}.API");

        if (!Directory.Exists(backendPath))
        {
            issues.Add($"Backend API project not found: {backendPath}");
        }

        // 2. Check that Frontend folder exists
        var frontendPath = Path.Combine(
            context.TargetPath,
            "Frontend",
            $"{context.NewProjectName}-frontend");

        if (!Directory.Exists(frontendPath))
        {
            issues.Add($"Frontend project not found: {frontendPath}");
        }

        // 3. Check for any remaining "WebTemplate" references in key files
        var solutionFiles = Directory.GetFiles(
            context.TargetPath,
            "*.sln",
            SearchOption.TopDirectoryOnly);

        foreach (var slnFile in solutionFiles)
        {
            var content = File.ReadAllText(slnFile);
            if (content.Contains("WebTemplate") &&
                !content.Contains($"{context.NewProjectName}"))
            {
                issues.Add($"Solution file still contains 'WebTemplate': {slnFile}");
            }
        }

        if (issues.Any())
        {
            return Task.FromResult(new StepResult(
                false,
                $"Validation found {issues.Count} issue(s): {string.Join("; ", issues)}"));
        }

        return Task.FromResult(new StepResult(true, "Project validation passed"));
    }
}
```

---

### Phase 7: Main Template Engine & Integration

**Component: TemplateEngine.cs**

```csharp
public class TemplateEngine : ITemplateEngine
{
    private readonly ILogger<TemplateEngine> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TemplateEngine(
        ILogger<TemplateEngine> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<GenerationResult> GenerateAsync(
        WorkspaceConfiguration config,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var stepResults = new List<StepResult>();

        try
        {
            // Build context
            var context = BuildContext(config);

            // Get all generation steps in order
            var steps = GetGenerationSteps();

            // Execute each step
            foreach (var step in steps)
            {
                var result = await step.ExecuteAsync(context, progress, cancellationToken);
                stepResults.Add(result);

                if (!result.Success)
                {
                    return new GenerationResult
                    {
                        Success = false,
                        Message = $"Step {step.StepNumber} failed: {result.Message}",
                        StepResults = stepResults,
                        Duration = sw.Elapsed
                    };
                }
            }

            sw.Stop();

            return new GenerationResult
            {
                Success = true,
                Message = "Project generated successfully",
                GeneratedPath = context.TargetPath,
                StepResults = stepResults,
                Duration = sw.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Template generation failed with exception");

            return new GenerationResult
            {
                Success = false,
                Message = $"Template generation failed: {ex.Message}",
                StepResults = stepResults,
                Duration = sw.Elapsed,
                Exception = ex
            };
        }
    }

    private TemplateContext BuildContext(WorkspaceConfiguration config)
    {
        var templateRoot = GetTemplateRoot();
        var targetPath = Path.GetFullPath(config.Project.TargetPath);
        var databaseName = $"{config.Project.ProjectName}Db";

        return new TemplateContext
        {
            Configuration = config,
            TemplatePath = templateRoot,
            TargetPath = targetPath,
            OldProjectName = "WebTemplate",
            NewProjectName = config.Project.ProjectName,
            DatabaseName = databaseName,
            ConnectionString = BuildConnectionString(databaseName)
        };
    }

    private IEnumerable<IGenerationStep> GetGenerationSteps()
    {
        // Resolve steps from DI in order
        return
        [
            _serviceProvider.GetRequiredService<ValidateTemplateStep>(),
            _serviceProvider.GetRequiredService<CopyFilesStep>(),
            _serviceProvider.GetRequiredService<RebrandProjectStep>(),
            _serviceProvider.GetRequiredService<UpdateConfigurationsStep>(),
            _serviceProvider.GetRequiredService<InitializeGitStep>(),
            _serviceProvider.GetRequiredService<ValidateProjectStep>()
        ];
    }

    private static string GetTemplateRoot()
    {
        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "WebTemplate.sln")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    private static string BuildConnectionString(string databaseName)
    {
        return $"Server=(localdb)\\mssqllocaldb;Database={databaseName};" +
               "Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true";
    }
}
```

**Update ProjectGenerationService.cs:**

```csharp
public class ProjectGenerationService
{
    private readonly ITemplateEngine _templateEngine;
    private readonly DatabaseService _databaseService;
    private readonly ILogger<ProjectGenerationService> _logger;

    public async Task<(bool Success, string Message, string? GeneratedPath)> GenerateProjectAsync(
        WorkspaceConfiguration config,
        IProgress<string>? progress = null)
    {
        try
        {
            // Validate configuration
            progress?.Report("Validating configuration...");
            var validation = config.Validate();
            if (!validation.IsValid)
            {
                return (false, $"Configuration validation failed:\n{string.Join("\n", validation.Errors)}", null);
            }

            // Ensure target doesn't exist
            var targetPath = Path.GetFullPath(config.Project.TargetPath);
            if (Directory.Exists(targetPath))
            {
                return (false, $"Target directory already exists: {targetPath}", null);
            }

            // Run template engine
            progress?.Report("Generating project from template...");
            var result = await _templateEngine.GenerateAsync(config, progress);

            if (!result.Success)
            {
                return (false, result.Message, null);
            }

            // Initialize database (if requested)
            if (config.Database.ExecuteInitScript)
            {
                progress?.Report("Initializing database...");
                var dbResult = await _databaseService.ExecuteInitScriptAsync(
                    config.Database.ConnectionString,
                    Path.Combine(targetPath, "Backend", $"{config.Project.ProjectName}.Data", "Migrations", "db-init.sql"));

                if (!dbResult.Success)
                {
                    return (false, $"Database initialization failed: {dbResult.Message}", targetPath);
                }
            }

            progress?.Report($"Project generated successfully in {result.Duration.TotalSeconds:F1}s");
            return (true, "Project generation completed successfully", result.GeneratedPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Project generation failed");
            return (false, $"Error: {ex.Message}", null);
        }
    }
}
```

**Dependency Injection Setup (Program.cs):**

```csharp
// Template Engine Services
builder.Services.AddSingleton<ITemplateEngine, TemplateEngine>();

// Generation Steps
builder.Services.AddTransient<ValidateTemplateStep>();
builder.Services.AddTransient<CopyFilesStep>();
builder.Services.AddTransient<RebrandProjectStep>();
builder.Services.AddTransient<UpdateConfigurationsStep>();
builder.Services.AddTransient<InitializeGitStep>();
builder.Services.AddTransient<ValidateProjectStep>();

// Helper Services
builder.Services.AddSingleton<FileCopier>();
builder.Services.AddSingleton<FileRebrander>();
builder.Services.AddSingleton<ConfigurationUpdater>();
builder.Services.AddSingleton<GitInitializer>();
```

---

## Migration Strategy

### Step 1: Parallel Implementation
- Keep PowerShell script functional
- Build C# implementation alongside
- Add feature flag to switch between implementations

### Step 2: Testing & Validation
- Unit test each step independently
- Integration test full pipeline
- Compare outputs between PS and C# versions
- Test error scenarios

### Step 3: Gradual Rollout
- Default to C# implementation
- Keep PS as fallback option
- Monitor for issues

### Step 4: Cleanup
- Remove PowerShell scripts
- Remove `pwsh` dependency
- Update documentation

---

## Benefits of C# Implementation

### For Development
1. **Type Safety** - Compile-time error detection
2. **IntelliSense** - Full IDE support with autocomplete
3. **Debugging** - Standard Visual Studio debugging with breakpoints
4. **Refactoring** - Safe renames, move operations
5. **Testing** - Unit test framework integration

### For Maintenance
1. **Single Language** - No need to know PowerShell
2. **Clear Structure** - Interface-based design
3. **Better Errors** - Stack traces with line numbers
4. **Logging** - Integrated ILogger support
5. **Async/Await** - Proper cancellation support

### For Users
1. **Faster Execution** - No PowerShell startup overhead
2. **Better Progress** - Real-time feedback
3. **Cancellation** - Responsive UI during long operations
4. **Error Recovery** - Better error messages
5. **No External Dependencies** - Pure .NET solution

---

## Testing Strategy

### Unit Tests

```csharp
public class FileCopierTests
{
    [Fact]
    public async Task CopyTemplateAsync_WithExclusions_SkipsExcludedFiles()
    {
        // Arrange
        var sourceDir = CreateTempDirectory();
        var targetDir = CreateTempDirectory();
        CreateFile(sourceDir, "include.txt");
        CreateFile(sourceDir, "exclude.log");

        var copier = new FileCopier();

        // Act
        var result = await copier.CopyTemplateAsync(
            sourceDir,
            targetDir,
            includeFolders: ["*"],
            excludeDirs: [],
            excludeFiles: ["*.log"],
            progress: null);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(Path.Combine(targetDir, "include.txt")));
        Assert.False(File.Exists(Path.Combine(targetDir, "exclude.log")));
    }
}
```

### Integration Tests

```csharp
public class TemplateEngineIntegrationTests
{
    [Fact]
    public async Task GenerateAsync_WithValidConfig_CreatesProject()
    {
        // Arrange
        var config = CreateTestConfiguration();
        var engine = CreateTemplateEngine();

        // Act
        var result = await engine.GenerateAsync(config);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.GeneratedPath);
        Assert.True(Directory.Exists(result.GeneratedPath));

        // Verify structure
        var apiPath = Path.Combine(result.GeneratedPath, "Backend", $"{config.Project.ProjectName}.API");
        Assert.True(Directory.Exists(apiPath));
    }
}
```

---

## Performance Considerations

1. **Parallel File Operations** - Use `Parallel.ForEachAsync` for independent file operations
2. **Memory Efficiency** - Stream large files instead of loading entirely
3. **Progress Reporting** - Batch progress updates (don't report every file)
4. **Cancellation** - Check `CancellationToken` in loops
5. **Caching** - Cache compiled regex patterns for content replacement

---

## Error Handling Strategy

```csharp
// Each step should:
1. Catch specific exceptions
2. Log detailed error information
3. Return StepResult with clear message
4. Allow engine to decide whether to continue

// Example:
public async Task<StepResult> ExecuteAsync(...)
{
    try
    {
        // Step logic
        return new StepResult(true, "Success message");
    }
    catch (IOException ex)
    {
        _logger.LogError(ex, "File operation failed in {StepName}", StepName);
        return new StepResult(false, $"File operation failed: {ex.Message}", ex);
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogError(ex, "Access denied in {StepName}", StepName);
        return new StepResult(false, $"Access denied: {ex.Message}", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error in {StepName}", StepName);
        return new StepResult(false, $"Unexpected error: {ex.Message}", ex);
    }
}
```

---

## Implementation Timeline

**Phase 1: Foundation** - 2-3 hours
- Interfaces, models, base engine

**Phase 2: File Operations** - 3-4 hours
- Copy logic, exclusion handling

**Phase 3: Rebranding** - 3-4 hours
- Rename structure, content replacement

**Phase 4: Configuration** - 2-3 hours
- JSON updates, file modifications

**Phase 5: Git** - 1-2 hours
- Git command execution

**Phase 6: Validation** - 1-2 hours
- Template & project validation

**Phase 7: Integration** - 2-3 hours
- Wire up DI, update UI

**Testing & Refinement** - 3-4 hours
- Unit tests, integration tests, bug fixes

**Total Estimated Time: 17-25 hours** (2-3 days of focused work)

---

## Success Criteria

✅ **Zero PowerShell dependencies**
✅ **All template generation works from C#**
✅ **Comprehensive unit test coverage (>80%)**
✅ **Clear error messages for all failure scenarios**
✅ **Cancellation works at any step**
✅ **Performance equal to or better than PowerShell**
✅ **Clean separation of concerns**
✅ **Easy to extend with new steps**

---

## Conclusion

This C# implementation will provide:
- **Type safety** and compile-time error detection
- **Better maintainability** with a single language
- **Improved debugging** experience
- **No external dependencies** (pure .NET)
- **Better error handling** and recovery
- **Extensibility** for future enhancements

The modular step-based design makes it easy to add new features, test individual components, and maintain the codebase going forward without requiring PowerShell knowledge.
