using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace WebTemplate.E2ETests;

/// <summary>
/// E2E tests for the full authentication flow against a real backend server.
///
/// Prerequisites:
/// - Backend server must be running at the configured URL
/// - Admin credentials must be valid
///
/// To run: Remove the Skip attribute from [Fact] annotations
/// </summary>
[Collection("E2E Tests")]
public class AuthFlowTests : E2ETestBase
{
    [Fact(Skip = "Requires running backend server. Remove Skip attribute to run.")]
    public async Task FullAuthFlow_RegisterLoginLogout_WorksCorrectly()
    {
        // Step 1: Register a new user
        var email = $"e2etest_{Guid.NewGuid():N}@example.com";
        var password = "Test123!";
        var (userId, registerToken) = await RegisterUserAsync(email, password, "E2E", "Test");

        userId.Should().NotBeNullOrEmpty();
        registerToken.Should().NotBeNullOrEmpty();

        // Step 2: Verify we can get user status with the registration token
        SetAuthToken(registerToken);
        var statusResponse = await Client.GetAsync("/api/auth/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var statusContent = await statusResponse.Content.ReadAsStringAsync();
        statusContent.Should().Contain(email);

        // Step 3: Logout
        var logoutResponse = await Client.PostAsync("/api/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Verify status now returns Unauthorized
        var statusAfterLogout = await Client.GetAsync("/api/auth/status");
        statusAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Step 5: Login again with credentials
        ClearAuthToken();
        var loginRequest = new { email, password };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        loginContent.Should().Contain("accessToken");
    }

    [Fact(Skip = "Requires running backend server. Remove Skip attribute to run.")]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new
        {
            email = "nonexistent@example.com",
            password = "WrongPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(Skip = "Requires running backend server. Remove Skip attribute to run.")]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange - Register first user
        var email = $"duplicate_{Guid.NewGuid():N}@example.com";
        await RegisterUserAsync(email, "Test123!", "First", "User");

        // Act - Try to register with same email
        var duplicateRequest = new
        {
            email,
            password = "AnotherPassword123!",
            firstName = "Second",
            lastName = "User"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(Skip = "Requires running backend server. Remove Skip attribute to run.")]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange - Login as admin to get refresh token
        var loginRequest = new
        {
            email = E2ETestConfig.AdminEmail,
            password = E2ETestConfig.AdminPassword
        };

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginJson = System.Text.Json.JsonDocument.Parse(loginContent);
        var refreshToken = loginJson.RootElement.GetProperty("data").GetProperty("refreshToken").GetString();

        refreshToken.Should().NotBeNullOrEmpty();

        // Act - Use refresh token to get new tokens
        var refreshRequest = new { refreshToken };
        var refreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshContent = await refreshResponse.Content.ReadAsStringAsync();
        refreshContent.Should().Contain("accessToken");
        refreshContent.Should().Contain("refreshToken");
    }
}
