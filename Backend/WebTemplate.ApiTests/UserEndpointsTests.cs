namespace WebTemplate.ApiTests
{
    using System.Net;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Xunit;

    /// <summary>
    /// Integration tests for User endpoints (UserController)
    /// Tests both user profile endpoints and admin user management endpoints
    /// </summary>
    [Collection("Integration Tests")]
    public class UserEndpointsTests : IAsyncLifetime
    {
        private readonly TestWebAppFactory _factory;
        private HttpClient _client = default!;
        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public UserEndpointsTests(TestWebAppFactory factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            await _factory.InitializeDatabaseAsync();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost/"),
                AllowAutoRedirect = false
            });
        }

        public Task DisposeAsync()
        {
            _client?.Dispose();
            return Task.CompletedTask;
        }

        #region Helper Methods

        private async Task<string> GetAdminTokenAsync()
        {
            var loginPayload = new { email = "admin@WebTemplate.com", password = "Admin123!" };
            var loginResp = await _client.PostAsJsonAsync("/api/auth/login", loginPayload, _json);
            loginResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            return loginJson.GetProperty("data").GetProperty("accessToken").GetString()!;
        }

        private async Task<(string userId, string token)> CreateTestUserAsync()
        {
            var email = $"testuser_{Guid.NewGuid():N}@example.com";
            var registerPayload = new
            {
                firstName = "Test",
                lastName = "User",
                email,
                password = "Test123!",
                confirmPassword = "Test123!",
                acceptTerms = true,
                acceptPrivacyPolicy = true
            };

            var regResp = await _client.PostAsJsonAsync("/api/auth/register", registerPayload, _json);
            regResp.IsSuccessStatusCode.Should().BeTrue();

            var loginPayload = new { email, password = "Test123!" };
            var loginResp = await _client.PostAsJsonAsync("/api/auth/login", loginPayload, _json);
            loginResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            var token = loginJson.GetProperty("data").GetProperty("accessToken").GetString()!;
            var userId = loginJson.GetProperty("data").GetProperty("user").GetProperty("id").GetString()!;

            return (userId, token);
        }

        private async Task<string> CreateTestUserWithAdminAsync()
        {
            var adminToken = await GetAdminTokenAsync();
            var email = $"testuser_{Guid.NewGuid():N}@example.com";

            var createPayload = new
            {
                email,
                firstName = "Created",
                lastName = "User",
                userTypeId = 2, // User type
                sendEmail = false
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "/api/user")
            {
                Content = JsonContent.Create(createPayload, options: _json)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var resp = await _client.SendAsync(req);
            resp.StatusCode.Should().Be(HttpStatusCode.Created);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            return json.GetProperty("data").GetString()!;
        }

        #endregion

        #region Profile Endpoints Tests

        [Fact(DisplayName = "GET /api/user/profile - Returns current user profile")]
        public async Task GetProfile_WithAuthenticatedUser_ReturnsProfile()
        {
            // Arrange
            var (userId, token) = await CreateTestUserAsync();

            var req = new HttpRequestMessage(HttpMethod.Get, "/api/user/profile");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();
            json.GetProperty("data").GetProperty("id").GetString().Should().Be(userId);
            json.GetProperty("data").GetProperty("firstName").GetString().Should().Be("Test");
            json.GetProperty("data").GetProperty("lastName").GetString().Should().Be("User");
        }

        [Fact(DisplayName = "GET /api/user/profile - Unauthorized without token")]
        public async Task GetProfile_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            var resp = await _client.GetAsync("/api/user/profile");

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "PUT /api/user/profile - Updates current user profile")]
        public async Task UpdateProfile_WithValidData_UpdatesProfile()
        {
            // Arrange
            var (userId, token) = await CreateTestUserAsync();

            var updatePayload = new
            {
                firstName = "Updated",
                lastName = "Name",
                phoneNumber = "+1234567890",
                address = "123 Test St",
                city = "Test City",
                postalCode = "12345",
                country = "Test Country"
            };

            var req = new HttpRequestMessage(HttpMethod.Put, "/api/user/profile")
            {
                Content = JsonContent.Create(updatePayload, options: _json)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();

            // Verify profile was updated
            var getReq = new HttpRequestMessage(HttpMethod.Get, "/api/user/profile");
            getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var getResp = await _client.SendAsync(getReq);
            var getJson = await getResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            getJson.GetProperty("data").GetProperty("firstName").GetString().Should().Be("Updated");
            getJson.GetProperty("data").GetProperty("lastName").GetString().Should().Be("Name");
            getJson.GetProperty("data").GetProperty("city").GetString().Should().Be("Test City");
        }

        [Fact(DisplayName = "POST /api/user/change-password - Changes user password")]
        public async Task ChangePassword_WithValidData_ChangesPassword()
        {
            // Arrange
            var (userId, token) = await CreateTestUserAsync();

            var changePayload = new
            {
                currentPassword = "Test123!",
                newPassword = "NewTest123!",
                confirmPassword = "NewTest123!"
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "/api/user/change-password")
            {
                Content = JsonContent.Create(changePayload, options: _json)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();
        }

        #endregion

        #region Admin User Management Tests

        [Fact(DisplayName = "GET /api/user - Admin can list all users")]
        public async Task GetAllUsers_AsAdmin_ReturnsUserList()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var req = new HttpRequestMessage(HttpMethod.Get, "/api/user");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();
            json.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
        }

        [Fact(DisplayName = "GET /api/user - Regular user cannot list users")]
        public async Task GetAllUsers_AsRegularUser_ReturnsForbidden()
        {
            // Arrange
            var (userId, token) = await CreateTestUserAsync();
            var req = new HttpRequestMessage(HttpMethod.Get, "/api/user");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact(DisplayName = "GET /api/user/{id} - Admin can get user by ID")]
        public async Task GetUserById_AsAdmin_ReturnsUser()
        {
            // Arrange
            var userId = await CreateTestUserWithAdminAsync();
            var adminToken = await GetAdminTokenAsync();

            var req = new HttpRequestMessage(HttpMethod.Get, $"/api/user/{userId}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();
            json.GetProperty("data").GetProperty("id").GetString().Should().Be(userId);
        }

        [Fact(DisplayName = "GET /api/user/{id} - Returns NotFound for non-existent user")]
        public async Task GetUserById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var fakeId = Guid.NewGuid().ToString();

            var req = new HttpRequestMessage(HttpMethod.Get, $"/api/user/{fakeId}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "POST /api/user - Admin can create new user")]
        public async Task CreateUser_AsAdmin_CreatesUser()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var email = $"newuser_{Guid.NewGuid():N}@example.com";

            var createPayload = new
            {
                email,
                firstName = "New",
                lastName = "User",
                userTypeId = 2,
                sendEmail = false
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "/api/user")
            {
                Content = JsonContent.Create(createPayload, options: _json)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();

            // Data contains the userId as a string
            var userId = json.GetProperty("data").GetString();
            userId.Should().NotBeNullOrEmpty();

            // Verify the user was created by fetching it
            var getReq = new HttpRequestMessage(HttpMethod.Get, $"/api/user/{userId}");
            getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            var getResp = await _client.SendAsync(getReq);
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var getJson = await getResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            getJson.GetProperty("data").GetProperty("email").GetString().Should().Be(email);
            getJson.GetProperty("data").GetProperty("firstName").GetString().Should().Be("New");
        }

        [Fact(DisplayName = "POST /api/user - Cannot create user with duplicate email")]
        public async Task CreateUser_WithDuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();

            var createPayload = new
            {
                email = "admin@WebTemplate.com", // Existing admin email
                firstName = "Duplicate",
                lastName = "User",
                userTypeId = 2,
                sendEmail = false
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "/api/user")
            {
                Content = JsonContent.Create(createPayload, options: _json)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "PUT /api/user/{id} - Admin can update user")]
        public async Task UpdateUser_AsAdmin_UpdatesUser()
        {
            // Arrange
            var userId = await CreateTestUserWithAdminAsync();
            var adminToken = await GetAdminTokenAsync();

            var updatePayload = new
            {
                firstName = "Modified",
                lastName = "TestUser",
                phoneNumber = "+9876543210",
                userTypeId = 2
            };

            var req = new HttpRequestMessage(HttpMethod.Put, $"/api/user/{userId}")
            {
                Content = JsonContent.Create(updatePayload, options: _json)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();
        }

        [Fact(DisplayName = "DELETE /api/user/{id} - Admin can soft delete user")]
        public async Task DeleteUser_AsAdmin_SoftDeletesUser()
        {
            // Arrange
            var userId = await CreateTestUserWithAdminAsync();
            var adminToken = await GetAdminTokenAsync();

            var req = new HttpRequestMessage(HttpMethod.Delete, $"/api/user/{userId}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify user is deleted (should return NotFound)
            var getReq = new HttpRequestMessage(HttpMethod.Get, $"/api/user/{userId}");
            getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            var getResp = await _client.SendAsync(getReq);
            getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "POST /api/user/{id}/activate - Admin can activate user")]
        public async Task ActivateUser_AsAdmin_ActivatesUser()
        {
            // Arrange
            var userId = await CreateTestUserWithAdminAsync();
            var adminToken = await GetAdminTokenAsync();

            // First deactivate
            var deactivateReq = new HttpRequestMessage(HttpMethod.Post, $"/api/user/{userId}/deactivate");
            deactivateReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            await _client.SendAsync(deactivateReq);

            // Then activate
            var req = new HttpRequestMessage(HttpMethod.Post, $"/api/user/{userId}/activate");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();
        }

        [Fact(DisplayName = "POST /api/user/{id}/deactivate - Admin can deactivate user")]
        public async Task DeactivateUser_AsAdmin_DeactivatesUser()
        {
            // Arrange
            var userId = await CreateTestUserWithAdminAsync();
            var adminToken = await GetAdminTokenAsync();

            var req = new HttpRequestMessage(HttpMethod.Post, $"/api/user/{userId}/deactivate");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();
        }

        [Fact(DisplayName = "Regular user cannot perform admin operations")]
        public async Task AdminOperations_AsRegularUser_ReturnsForbidden()
        {
            // Arrange
            var (userId, userToken) = await CreateTestUserAsync();
            var targetUserId = await CreateTestUserWithAdminAsync();

            // Act & Assert - Try to delete another user
            var deleteReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/user/{targetUserId}");
            deleteReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
            var deleteResp = await _client.SendAsync(deleteReq);
            deleteResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            // Act & Assert - Try to create user
            var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/user")
            {
                Content = JsonContent.Create(new
                {
                    email = $"test_{Guid.NewGuid():N}@example.com",
                    firstName = "Test",
                    lastName = "User",
                    userTypeId = 2,
                    sendEmail = false
                }, options: _json)
            };
            createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
            var createResp = await _client.SendAsync(createReq);
            createResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion
    }
}
