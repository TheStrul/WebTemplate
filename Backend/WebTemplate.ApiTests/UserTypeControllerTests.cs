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
    /// Integration tests for UserType endpoints (UserTypeController)
    /// Tests CRUD operations, authorization, and validation
    /// Requires Admin role for all endpoints
    /// </summary>
    [Collection("Integration Tests")]
    public class UserTypeControllerTests
    {
        private readonly TestWebAppFactory _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public UserTypeControllerTests()
        {
            _factory = new TestWebAppFactory();
            _factory.InitializeDatabaseAsync().GetAwaiter().GetResult();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost/"),
                AllowAutoRedirect = false
            });
        }

        #region Helper Methods

        /// <summary>
        /// Gets admin JWT token for authenticated requests
        /// </summary>
        private async Task<string> GetAdminTokenAsync()
        {
            var loginPayload = new { email = "admin@WebTemplate.com", password = "Admin123!@#" };
            var loginResp = await _client.PostAsJsonAsync("/api/auth/login", loginPayload, _json);
            loginResp.StatusCode.Should().Be(HttpStatusCode.OK, "admin should be seeded");

            var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            return loginJson.GetProperty("data").GetProperty("accessToken").GetString()!;
        }

        /// <summary>
        /// Creates authenticated request with bearer token
        /// </summary>
        private HttpRequestMessage CreateAuthenticatedRequest(HttpMethod method, string url, string? token = null)
        {
            var req = new HttpRequestMessage(method, url);
            if (token != null)
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return req;
        }

        #endregion

        #region GET /api/usertype - GetAllUserTypes

        [Fact(DisplayName = "GET /api/usertype - Admin retrieves all user types")]
        public async Task GetAllUserTypes_AsAdmin_ReturnsAllUserTypes()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var req = CreateAuthenticatedRequest(HttpMethod.Get, "/api/usertype", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();
            var data = json.GetProperty("data");
            data.GetArrayLength().Should().BeGreaterThanOrEqualTo(3, "should have seeded user types (Admin, User, Moderator)");
        }

        [Fact(DisplayName = "GET /api/usertype - Response includes all required fields")]
        public async Task GetAllUserTypes_ReturnsCompleteUserTypeData()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var req = CreateAuthenticatedRequest(HttpMethod.Get, "/api/usertype", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            var userTypes = json.GetProperty("data");
            var firstUserType = userTypes[0];

            firstUserType.GetProperty("id").GetInt32().Should().BeGreaterThan(0);
            firstUserType.GetProperty("name").GetString().Should().NotBeNullOrWhiteSpace();
            firstUserType.GetProperty("description").GetString().Should().NotBeNullOrEmpty();
            firstUserType.GetProperty("isActive").GetBoolean().Should().BeTrue();
        }

        [Fact(DisplayName = "GET /api/usertype - Unauthorized without token")]
        public async Task GetAllUserTypes_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            var resp = await _client.GetAsync("/api/usertype");

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "GET /api/usertype - Forbidden for non-admin user")]
        public async Task GetAllUserTypes_AsRegularUser_ReturnsForbidden()
        {
            // Arrange - Create regular user
            var email = $"user_{Guid.NewGuid():N}@example.com";
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
            await _client.PostAsJsonAsync("/api/auth/register", registerPayload, _json);

            var loginPayload = new { email, password = "Test123!" };
            var loginResp = await _client.PostAsJsonAsync("/api/auth/login", loginPayload, _json);
            var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            var userToken = loginJson.GetProperty("data").GetProperty("accessToken").GetString()!;

            var req = CreateAuthenticatedRequest(HttpMethod.Get, "/api/usertype", userToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region GET /api/usertype/{id} - GetUserTypeById

        [Fact(DisplayName = "GET /api/usertype/{id} - Admin retrieves user type by ID")]
        public async Task GetUserTypeById_WithValidId_ReturnsUserType()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var id = 1; // Admin user type ID
            var req = CreateAuthenticatedRequest(HttpMethod.Get, $"/api/usertype/{id}", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();
            json.GetProperty("data").GetProperty("id").GetInt32().Should().Be(id);
            json.GetProperty("data").GetProperty("name").GetString().Should().Be("Admin");
        }

        [Fact(DisplayName = "GET /api/usertype/{id} - Returns NotFound for non-existent ID")]
        public async Task GetUserTypeById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var nonExistentId = 9999;
            var req = CreateAuthenticatedRequest(HttpMethod.Get, $"/api/usertype/{nonExistentId}", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeFalse();
            json.GetProperty("message").GetString().Should().Contain("not found");
        }

        [Fact(DisplayName = "GET /api/usertype/{id} - Unauthorized without token")]
        public async Task GetUserTypeById_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            var resp = await _client.GetAsync("/api/usertype/1");

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region POST /api/usertype - CreateUserType

        [Fact(DisplayName = "POST /api/usertype - Admin creates new user type")]
        public async Task CreateUserType_WithValidData_CreatesUserType()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var createPayload = new
            {
                name = $"TestType_{Guid.NewGuid():N}",
                description = "Test user type for integration testing"
            };

            var req = CreateAuthenticatedRequest(HttpMethod.Post, "/api/usertype", adminToken);
            req.Content = JsonContent.Create(createPayload, options: _json);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();
            var newId = json.GetProperty("data").GetInt32();
            newId.Should().BeGreaterThan(0);

            // Verify the user type was created
            var verifyReq = CreateAuthenticatedRequest(HttpMethod.Get, $"/api/usertype/{newId}", adminToken);
            var verifyResp = await _client.SendAsync(verifyReq);
            verifyResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var verifyJson = await verifyResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            verifyJson.GetProperty("data").GetProperty("name").GetString().Should().Be(createPayload.name);
            verifyJson.GetProperty("data").GetProperty("description").GetString().Should().Be(createPayload.description);
        }

        [Fact(DisplayName = "POST /api/usertype - Cannot create duplicate user type")]
        public async Task CreateUserType_WithDuplicateName_ReturnsBadRequest()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var createPayload = new
            {
                name = "Admin", // Already exists
                description = "Duplicate"
            };

            var req = CreateAuthenticatedRequest(HttpMethod.Post, "/api/usertype", adminToken);
            req.Content = JsonContent.Create(createPayload, options: _json);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeFalse();
        }

        [Fact(DisplayName = "POST /api/usertype - Validates required fields")]
        public async Task CreateUserType_WithMissingData_ReturnsBadRequest()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var createPayload = new
            {
                name = "" // Empty name
            };

            var req = CreateAuthenticatedRequest(HttpMethod.Post, "/api/usertype", adminToken);
            req.Content = JsonContent.Create(createPayload, options: _json);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeFalse();
            json.GetProperty("message").GetString().Should().Contain("Invalid");
        }

        [Fact(DisplayName = "POST /api/usertype - Forbidden for non-admin user")]
        public async Task CreateUserType_AsRegularUser_ReturnsForbidden()
        {
            // Arrange - Create regular user
            var email = $"user_{Guid.NewGuid():N}@example.com";
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
            await _client.PostAsJsonAsync("/api/auth/register", registerPayload, _json);

            var loginPayload = new { email, password = "Test123!" };
            var loginResp = await _client.PostAsJsonAsync("/api/auth/login", loginPayload, _json);
            var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            var userToken = loginJson.GetProperty("data").GetProperty("accessToken").GetString()!;

            var createPayload = new
            {
                name = "UnauthorizedType",
                description = "Should fail"
            };

            var req = CreateAuthenticatedRequest(HttpMethod.Post, "/api/usertype", userToken);
            req.Content = JsonContent.Create(createPayload, options: _json);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region PUT /api/usertype/{id} - UpdateUserType

        [Fact(DisplayName = "PUT /api/usertype/{id} - Admin updates user type")]
        public async Task UpdateUserType_WithValidData_UpdatesUserType()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            
            // Create a new user type first
            var createPayload = new
            {
                name = $"UpdateTest_{Guid.NewGuid():N}",
                description = "Original description"
            };

            var createReq = CreateAuthenticatedRequest(HttpMethod.Post, "/api/usertype", adminToken);
            createReq.Content = JsonContent.Create(createPayload, options: _json);
            var createResp = await _client.SendAsync(createReq);
            var createJson = await createResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            var userTypeId = createJson.GetProperty("data").GetInt32();

            // Update it
            var updatePayload = new
            {
                name = $"UpdateTest_{Guid.NewGuid():N}",
                description = "Updated description"
            };

            var updateReq = CreateAuthenticatedRequest(HttpMethod.Put, $"/api/usertype/{userTypeId}", adminToken);
            updateReq.Content = JsonContent.Create(updatePayload, options: _json);

            // Act
            var resp = await _client.SendAsync(updateReq);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();

            // Verify update
            var verifyReq = CreateAuthenticatedRequest(HttpMethod.Get, $"/api/usertype/{userTypeId}", adminToken);
            var verifyResp = await _client.SendAsync(verifyReq);
            var verifyJson = await verifyResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            verifyJson.GetProperty("data").GetProperty("description").GetString().Should().Be("Updated description");
        }

        [Fact(DisplayName = "PUT /api/usertype/{id} - Returns NotFound for non-existent ID")]
        public async Task UpdateUserType_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var updatePayload = new
            {
                name = "NonExistent",
                description = "Should fail"
            };

            var req = CreateAuthenticatedRequest(HttpMethod.Put, "/api/usertype/9999", adminToken);
            req.Content = JsonContent.Create(updatePayload, options: _json);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region DELETE /api/usertype/{id} - DeleteUserType

        [Fact(DisplayName = "DELETE /api/usertype/{id} - Admin deletes user type")]
        public async Task DeleteUserType_WithValidId_DeletesUserType()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            
            // Create a user type to delete
            var createPayload = new
            {
                name = $"DeleteTest_{Guid.NewGuid():N}",
                description = "Will be deleted"
            };

            var createReq = CreateAuthenticatedRequest(HttpMethod.Post, "/api/usertype", adminToken);
            createReq.Content = JsonContent.Create(createPayload, options: _json);
            var createResp = await _client.SendAsync(createReq);
            var createJson = await createResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            var userTypeId = createJson.GetProperty("data").GetInt32();

            // Delete it
            var deleteReq = CreateAuthenticatedRequest(HttpMethod.Delete, $"/api/usertype/{userTypeId}", adminToken);

            // Act
            var resp = await _client.SendAsync(deleteReq);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();

            // Verify deletion
            var verifyReq = CreateAuthenticatedRequest(HttpMethod.Get, $"/api/usertype/{userTypeId}", adminToken);
            var verifyResp = await _client.SendAsync(verifyReq);
            verifyResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "DELETE /api/usertype/{id} - Returns NotFound for non-existent ID")]
        public async Task DeleteUserType_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var req = CreateAuthenticatedRequest(HttpMethod.Delete, "/api/usertype/9999", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "DELETE /api/usertype/{id} - Forbidden for non-admin user")]
        public async Task DeleteUserType_AsRegularUser_ReturnsForbidden()
        {
            // Arrange - Create regular user
            var email = $"user_{Guid.NewGuid():N}@example.com";
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
            await _client.PostAsJsonAsync("/api/auth/register", registerPayload, _json);

            var loginPayload = new { email, password = "Test123!" };
            var loginResp = await _client.PostAsJsonAsync("/api/auth/login", loginPayload, _json);
            var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            var userToken = loginJson.GetProperty("data").GetProperty("accessToken").GetString()!;

            var req = CreateAuthenticatedRequest(HttpMethod.Delete, "/api/usertype/1", userToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region POST /api/usertype/{id}/toggle-status - ToggleUserTypeStatus

        [Fact(DisplayName = "POST /api/usertype/{id}/toggle-status - Admin toggles user type status")]
        public async Task ToggleUserTypeStatus_WithValidId_ToggleStatus()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            
            // Create a user type
            var createPayload = new
            {
                name = $"ToggleTest_{Guid.NewGuid():N}",
                description = "For toggling"
            };

            var createReq = CreateAuthenticatedRequest(HttpMethod.Post, "/api/usertype", adminToken);
            createReq.Content = JsonContent.Create(createPayload, options: _json);
            var createResp = await _client.SendAsync(createReq);
            var createJson = await createResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            var userTypeId = createJson.GetProperty("data").GetInt32();

            // Get initial state
            var getReq1 = CreateAuthenticatedRequest(HttpMethod.Get, $"/api/usertype/{userTypeId}", adminToken);
            var getResp1 = await _client.SendAsync(getReq1);
            var getJson1 = await getResp1.Content.ReadFromJsonAsync<JsonElement>(_json);
            var initialStatus = getJson1.GetProperty("data").GetProperty("isActive").GetBoolean();

            // Toggle status
            var toggleReq = CreateAuthenticatedRequest(HttpMethod.Post, $"/api/usertype/{userTypeId}/toggle-status", adminToken);

            // Act
            var resp = await _client.SendAsync(toggleReq);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            json.GetProperty("success").GetBoolean().Should().BeTrue();

            // Verify status changed
            var getReq2 = CreateAuthenticatedRequest(HttpMethod.Get, $"/api/usertype/{userTypeId}", adminToken);
            var getResp2 = await _client.SendAsync(getReq2);
            var getJson2 = await getResp2.Content.ReadFromJsonAsync<JsonElement>(_json);
            var newStatus = getJson2.GetProperty("data").GetProperty("isActive").GetBoolean();
            newStatus.Should().NotBe(initialStatus);
        }

        [Fact(DisplayName = "POST /api/usertype/{id}/toggle-status - Returns NotFound for non-existent ID")]
        public async Task ToggleUserTypeStatus_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            var req = CreateAuthenticatedRequest(HttpMethod.Post, "/api/usertype/9999/toggle-status", adminToken);

            // Act
            var resp = await _client.SendAsync(req);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "POST /api/usertype/{id}/toggle-status - Unauthorized without token")]
        public async Task ToggleUserTypeStatus_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            var resp = await _client.PostAsync("/api/usertype/1/toggle-status", null);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion
    }
}
