namespace WebTemplate.SearchTool;

using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// WebTemplate Search Tool - CLI for searching and generating markdown documentation
/// 
/// Usage:
///   WebTemplate.SearchTool ".NET 9 release notes breaking changes"
///   WebTemplate.SearchTool "EF Core 9 best practices"
/// 
/// Set API key via environment variable:
///   $env:TAVILY_API_KEY = "your-api-key"
/// 
/// Get free API key from: https://tavily.com
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Validate input
            if (args.Length == 0)
            {
                DisplayUsage();
                Environment.Exit(1);
                return;
            }

            // Get API key from environment
            var apiKey = Environment.GetEnvironmentVariable("TAVILY_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: TAVILY_API_KEY environment variable not set");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Get a free API key from: https://tavily.com");
                Console.WriteLine();
                Console.WriteLine("Set environment variable:");
                Console.WriteLine("  $env:TAVILY_API_KEY = \"your-api-key\"");
                Environment.Exit(1);
                return;
            }

            // Construct search query from arguments
            string query = string.Join(" ", args);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Searching for: {query}");
            Console.ResetColor();
            Console.WriteLine();

            // Perform search
            var client = new SearchClient(apiKey);
            var results = await client.SearchAsync(query, maxResults: 5);

            // Display results
            Console.WriteLine(results);
            Console.WriteLine();

            // Save to file
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"search_results_{timestamp}.md";
            await File.WriteAllTextAsync(filename, results);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Results saved to: {filename}");
            Console.ResetColor();
        }
        catch (ArgumentException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Argument Error: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Operation Error: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Unexpected Error: {ex.GetType().Name}: {ex.Message}");
            Console.ResetColor();
            if (!string.IsNullOrWhiteSpace(ex.InnerException?.Message))
            {
                Console.WriteLine($"Details: {ex.InnerException.Message}");
            }
            Environment.Exit(1);
        }
    }

    static void DisplayUsage()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("WebTemplate Search Tool");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  WebTemplate.SearchTool \"search query here\"");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  WebTemplate.SearchTool \".NET 9 release notes breaking changes\"");
        Console.WriteLine("  WebTemplate.SearchTool \"EF Core 9 best practices 2025\"");
        Console.WriteLine("  WebTemplate.SearchTool \"ASP.NET Core 9 new features\"");
        Console.WriteLine();
        Console.WriteLine("Setup:");
        Console.WriteLine("  1. Get free API key from: https://tavily.com");
        Console.WriteLine("  2. Set environment variable: $env:TAVILY_API_KEY = \"your-key\"");
        Console.WriteLine();
    }
}
