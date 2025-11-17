using Microsoft.Extensions.Logging;
using System.Text;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine.Steps;

/// <summary>
/// Step 4: Generates a solution file (.sln) for the new project
/// </summary>
public class GenerateSolutionFileStep : GenerationStepBase
{
    public override string StepName => "Generating Solution File";
    public override int StepNumber => 4;

    public GenerateSolutionFileStep(ILogger<GenerateSolutionFileStep> logger)
        : base(logger)
    {
    }

    public override Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ReportStepProgress(progress);

            Logger.LogInformation("Generating solution file for project: {ProjectName}", context.NewProjectName);

            var solutionPath = Path.Combine(context.TargetPath, $"{context.NewProjectName}.sln");

            // Generate solution file content
            var solutionContent = GenerateSolutionContent(context);

            // Write the solution file
            File.WriteAllText(solutionPath, solutionContent, new UTF8Encoding(false));

            var message = $"Solution file created: {Path.GetFileName(solutionPath)}";
            Logger.LogInformation(message);

            return Task.FromResult(new StepResult(true, message));
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "Solution file generation was cancelled");
            return Task.FromResult(new StepResult(false, "Solution file generation was cancelled", ex));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Solution file generation failed with unexpected error");
            return Task.FromResult(new StepResult(false, $"Solution file generation failed: {ex.Message}", ex));
        }
    }

    private static string GenerateSolutionContent(TemplateContext context)
    {
        var projectName = context.NewProjectName;

        // Generate unique GUIDs for projects
        var apiProjectGuid = Guid.NewGuid().ToString("B").ToUpperInvariant();
        var coreProjectGuid = Guid.NewGuid().ToString("B").ToUpperInvariant();
        var dataProjectGuid = Guid.NewGuid().ToString("B").ToUpperInvariant();
        var frontendProjectGuid = Guid.NewGuid().ToString("B").ToUpperInvariant();

        // Generate unique GUIDs for solution folders
        var backendFolderGuid = Guid.NewGuid().ToString("B").ToUpperInvariant();
        var frontendFolderGuid = Guid.NewGuid().ToString("B").ToUpperInvariant();

        var sb = new StringBuilder();

        // Solution header
        sb.AppendLine();
        sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
        sb.AppendLine("# Visual Studio Version 17");
        sb.AppendLine("VisualStudioVersion = 17.0.31903.59");
        sb.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");

        // Backend projects
        sb.AppendLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{projectName}.API\", \"Backend\\{projectName}.API\\{projectName}.API.csproj\", \"{apiProjectGuid}\"");
        sb.AppendLine("EndProject");
        sb.AppendLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{projectName}.Core\", \"Backend\\{projectName}.Core\\{projectName}.Core.csproj\", \"{coreProjectGuid}\"");
        sb.AppendLine("EndProject");
        sb.AppendLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{projectName}.Data\", \"Backend\\{projectName}.Data\\{projectName}.Data.csproj\", \"{dataProjectGuid}\"");
        sb.AppendLine("EndProject");

        // Frontend project
        sb.AppendLine($"Project(\"{{9A19103F-16F7-4668-BE54-9A1E7A4F7556}}\") = \"{projectName}.Frontend\", \"Frontend\\{projectName}-frontend\\{projectName}.Frontend.csproj\", \"{frontendProjectGuid}\"");
        sb.AppendLine("EndProject");

        // Solution folders
        sb.AppendLine($"Project(\"{{2150E333-8FDC-42A3-9474-1A3956D46DE8}}\") = \"Backend\", \"Backend\", \"{backendFolderGuid}\"");
        sb.AppendLine("EndProject");
        sb.AppendLine($"Project(\"{{2150E333-8FDC-42A3-9474-1A3956D46DE8}}\") = \"Frontend\", \"Frontend\", \"{frontendFolderGuid}\"");
        sb.AppendLine("EndProject");

        // Global section
        sb.AppendLine("Global");

        // Solution configuration platforms
        sb.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
        sb.AppendLine("\t\tDebug|Any CPU = Debug|Any CPU");
        sb.AppendLine("\t\tRelease|Any CPU = Release|Any CPU");
        sb.AppendLine("\tEndGlobalSection");

        // Project configuration platforms
        sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

        // API project configurations
        AppendProjectConfigurations(sb, apiProjectGuid);

        // Core project configurations
        AppendProjectConfigurations(sb, coreProjectGuid);

        // Data project configurations
        AppendProjectConfigurations(sb, dataProjectGuid);

        // Frontend project configurations
        AppendProjectConfigurations(sb, frontendProjectGuid);

        sb.AppendLine("\tEndGlobalSection");

        // Solution properties
        sb.AppendLine("\tGlobalSection(SolutionProperties) = preSolution");
        sb.AppendLine("\t\tHideSolutionNode = FALSE");
        sb.AppendLine("\tEndGlobalSection");

        // Nested projects (organize projects into solution folders)
        sb.AppendLine("\tGlobalSection(NestedProjects) = preSolution");
        sb.AppendLine($"\t\t{apiProjectGuid} = {backendFolderGuid}");
        sb.AppendLine($"\t\t{coreProjectGuid} = {backendFolderGuid}");
        sb.AppendLine($"\t\t{dataProjectGuid} = {backendFolderGuid}");
        sb.AppendLine($"\t\t{frontendProjectGuid} = {frontendFolderGuid}");
        sb.AppendLine("\tEndGlobalSection");

        // Extensibility globals
        sb.AppendLine("\tGlobalSection(ExtensibilityGlobals) = postSolution");
        sb.AppendLine($"\t\tSolutionGuid = {Guid.NewGuid().ToString("B").ToUpperInvariant()}");
        sb.AppendLine("\tEndGlobalSection");

        sb.AppendLine("EndGlobal");

        return sb.ToString();
    }

    private static void AppendProjectConfigurations(StringBuilder sb, string projectGuid)
    {
        sb.AppendLine($"\t\t{projectGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
        sb.AppendLine($"\t\t{projectGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU");
        sb.AppendLine($"\t\t{projectGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU");
        sb.AppendLine($"\t\t{projectGuid}.Release|Any CPU.Build.0 = Release|Any CPU");
    }
}
