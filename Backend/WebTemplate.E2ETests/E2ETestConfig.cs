using System.Text.Json;

namespace WebTemplate.E2ETests;

/// <summary>
/// Configuration for E2E tests that run against a real backend server.
/// Set the backend URL via environment variable or appsettings.json
/// </summary>
public class E2ETestConfig
{
    public static string BaseUrl { get; set; } =
        Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://localhost:5294";

    public static string AdminEmail { get; set; } =
        Environment.GetEnvironmentVariable("E2E_ADMIN_EMAIL") ?? "admin@WebTemplate.com";

    public static string AdminPassword { get; set; } =
        Environment.GetEnvironmentVariable("E2E_ADMIN_PASSWORD") ?? "Admin123!";

    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web);
}
