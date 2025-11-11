namespace WebTemplate.Data.Validation
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using WebTemplate.Data.Context;

    /// <summary>
    /// Standalone tool to validate DbContext configuration
    /// Run this to ensure 100% match between entity definitions and DbContext
    /// </summary>
    public class ValidateDbContextTool
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("WebTemplate - DbContext Validation Tool");
            Console.WriteLine("Verifies 100% match between entity definitions and ApplicationDbContext configuration");
            Console.WriteLine();

            try
            {
                // Find appsettings.json - check multiple locations
                var basePath = Directory.GetCurrentDirectory();
                var assemblyPath = Path.GetDirectoryName(typeof(ValidateDbContextTool).Assembly.Location);

                // Try to find appsettings in current dir, assembly dir, or up the tree
                string? settingsPath = null;
                foreach (var candidate in new[] { basePath, assemblyPath ?? basePath })
                {
                    if (File.Exists(Path.Combine(candidate, "appsettings.json")))
                    {
                        settingsPath = candidate;
                        break;
                    }
                }

                if (settingsPath == null)
                {
                    Console.WriteLine("WARNING: Could not find appsettings.json");
                    Console.WriteLine($"Searched in: {basePath}");
                    if (assemblyPath != null) Console.WriteLine($"         and: {assemblyPath}");
                }

                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(settingsPath ?? basePath)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("ERROR: No connection string found in configuration");
                    Console.WriteLine("Looking for: ConnectionStrings:DefaultConnection");
                    return 1;
                }

                Console.WriteLine($"Using connection string: {MaskConnectionString(connectionString)}");
                Console.WriteLine();

                // Create DbContext with options
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                using var context = new ApplicationDbContext(options);

                // Run validation
                var validator = new DbContextValidator(context);
                var result = validator.Validate();

                // Print report
                Console.WriteLine();
                Console.WriteLine(result.GetReport());

                // Return exit code
                return result.IsValid ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 2;
            }
        }

        private static string MaskConnectionString(string connectionString)
        {
            // Simple masking for display purposes
            var parts = connectionString.Split(';');
            var masked = parts.Select(part =>
            {
                if (part.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                    part.Contains("Pwd", StringComparison.OrdinalIgnoreCase))
                {
                    return part.Split('=')[0] + "=***";
                }
                return part;
            });
            return string.Join(";", masked);
        }
    }
}
