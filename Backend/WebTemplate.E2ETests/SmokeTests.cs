namespace WebTemplate.E2ETests
{
    using System.Net;
    using FluentAssertions;

    /// <summary>
    /// Smoke tests to verify the backend server is running and responsive.
    /// These tests should run quickly and catch basic deployment issues.
    ///
    /// Prerequisites:
    /// - Backend server must be running (auto-detected from launchSettings.json)
    /// - E2E_ADMIN_PASSWORD environment variable must be set
    ///
    /// Configuration (NO FALLBACKS - explicit values required):
    /// - E2E_BASE_URL: Override auto-detected server URL (optional)
    /// - E2E_ADMIN_EMAIL: Admin email (default: admin@WebTemplate.com)
    /// - E2E_ADMIN_PASSWORD: Admin password (REQUIRED - no default)
    /// </summary>
    [Collection("E2E Tests")]
    public class SmokeTests : E2ETestBase
    {
        [Fact]
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

        [Fact]
        public async Task Auth_AdminLogin_Succeeds()
        {
            // Arrange & Act
            var token = await LoginAsAdminAsync();

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Auth_RegisterNewUser_Succeeds()
        {
            // Arrange
            var email = $"smoketest_{Guid.NewGuid():N}@example.com";
            var password = "Test123!";

            // Act
            var (userId, accessToken, refreshToken) = await RegisterUserAsync(email, password);

            // Assert
            userId.Should().NotBeNullOrEmpty();
            accessToken.Should().NotBeNullOrEmpty();
            refreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
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
            content.Should().Contain(Config.Admin.Email);
        }

        [Fact]
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
}
