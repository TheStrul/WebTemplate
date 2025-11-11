using System.Net;
using FluentAssertions;

namespace WebTemplate.E2ETests;

/// <summary>
/// Smoke tests to verify the backend server is running and responsive.
/// These tests should run quickly and catch basic deployment issues.
///
/// Prerequisites:
/// - Backend server must be running at the configured URL (default: https://localhost:7001)
/// - Admin credentials must be valid
///
/// To run against a different environment:
/// - Set E2E_BASE_URL environment variable (e.g., "https://staging.example.com")
/// - Set E2E_ADMIN_EMAIL and E2E_ADMIN_PASSWORD if different from defaults
/// </summary>
[Collection("E2E Tests")]
public class SmokeTests : E2ETestBase
{
    [Fact(Skip = "Requires running backend server. Remove Skip attribute to run.")]
    public async Task Backend_IsRunning_AndResponsive()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/api/auth/status");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,           // If authenticated
            HttpStatusCode.Unauthorized  // If not authenticated (expected)
        );
    }

    [Fact(Skip = "Requires running backend server. Remove Skip attribute to run.")]
    public async Task Auth_AdminLogin_Succeeds()
    {
        // Arrange & Act
        var token = await LoginAsAdminAsync();

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact(Skip = "Requires running backend server. Remove Skip attribute to run.")]
    public async Task Auth_RegisterNewUser_Succeeds()
    {
        // Arrange
        var email = $"smoketest_{Guid.NewGuid():N}@example.com";
        var password = "Test123!";

        // Act
        var (userId, token) = await RegisterUserAsync(email, password);

        // Assert
        userId.Should().NotBeNullOrEmpty();
        token.Should().NotBeNullOrEmpty();
    }

    [Fact(Skip = "Requires running backend server. Remove Skip attribute to run.")]
    public async Task Auth_GetStatus_WithValidToken_ReturnsUserInfo()
    {
        // Arrange
        var token = await LoginAsAdminAsync();
        SetAuthToken(token);

        // Act
        var response = await Client.GetAsync("/api/auth/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("email");
        content.Should().Contain(E2ETestConfig.AdminEmail);
    }

    [Fact(Skip = "Requires running backend server. Remove Skip attribute to run.")]
    public async Task User_GetProfile_WithValidToken_ReturnsProfile()
    {
        // Arrange
        var token = await LoginAsAdminAsync();
        SetAuthToken(token);

        // Act
        var response = await Client.GetAsync("/api/user/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("firstName");
        content.Should().Contain("lastName");
        content.Should().Contain("email");
    }
}
