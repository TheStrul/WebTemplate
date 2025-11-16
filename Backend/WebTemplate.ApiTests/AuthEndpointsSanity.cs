namespace WebTemplate.ApiTests
{
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;
    using FluentAssertions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using WebTemplate.Core.Entities;
    using Xunit;

    [Collection("Integration Tests")]
    public class AuthEndpointsSanity
    {
        private readonly TestWebAppFactory _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public AuthEndpointsSanity(TestWebAppFactory factory)
        {
            _factory = factory;

            // Initialize database before creating client
            factory.InitializeDatabaseAsync().GetAwaiter().GetResult();

            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost/"),
                AllowAutoRedirect = false
            });
        }

        private async Task<T> WithScope<T>(Func<IServiceProvider, Task<T>> action)
        {
            using var scope = _factory.Services.CreateScope();
            return await action(scope.ServiceProvider);
        }

        private Task WithScope(Func<IServiceProvider, Task> action) => WithScope<object>(async sp => { await action(sp); return new object(); });

        [Fact(DisplayName = "Register -> Login -> Status (happy path)")]
        public async Task Register_Login_Status_HappyPath()
        {
            // Register
            var email = $"sanity_{Guid.NewGuid():N}@example.com";
            var registerPayload = new
            {
                firstName = "Sanity",
                lastName = "User",
                email,
                password = "Admin123!",
                confirmPassword = "Admin123!",
                acceptTerms = true,
                acceptPrivacyPolicy = true
            };
            var regResp = await _client.PostAsJsonAsync("/api/auth/register", registerPayload, _json);
            regResp.IsSuccessStatusCode.Should().BeTrue("registration should succeed without email confirmation in test env");

            // Login
            var loginPayload = new { email, password = "Admin123!" };
            var loginResp = await _client.PostAsJsonAsync("/api/auth/login", loginPayload, _json);
            loginResp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            loginJson.GetProperty("success").GetBoolean().Should().BeTrue();
            var accessToken = loginJson.GetProperty("data").GetProperty("accessToken").GetString();
            accessToken.Should().NotBeNullOrWhiteSpace();

            // Status
            var req = new HttpRequestMessage(HttpMethod.Get, "/api/auth/status");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var statusResp = await _client.SendAsync(req);
            statusResp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var statusJson = await statusResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            statusJson.GetProperty("success").GetBoolean().Should().BeTrue();
            statusJson.GetProperty("data").GetProperty("isAuthenticated").GetBoolean().Should().BeTrue();
        }

        [Fact(DisplayName = "Refresh -> Logout -> ChangePassword (authorized path)")]
        public async Task Refresh_Logout_ChangePassword_Authorized()
        {
            // Login as admin seeded user
            var loginPayload = new { email = "admin@WebTemplate.com", password = "Admin123!@#" };
            var loginResp = await _client.PostAsJsonAsync("/api/auth/login", loginPayload, _json);
            loginResp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "admin should be seeded in dev");

            var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>(_json);
            var accessToken = loginJson.GetProperty("data").GetProperty("accessToken").GetString()!;
            var refreshToken = loginJson.GetProperty("data").GetProperty("refreshToken").GetString()!;

            // Refresh
            var refreshPayload = new { refreshToken, accessToken };
            var refreshResp = await _client.PostAsJsonAsync("/api/auth/refresh-token", refreshPayload, _json);
            refreshResp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // Change password (will likely fail validation, but endpoint should be authorized)
            var changeReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/change-password")
            {
                Content = JsonContent.Create(new { currentPassword = "Admin123!@#", newPassword = "Admin123!@#", confirmPassword = "Admin123!@#" }, options: _json)
            };
            changeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var changeResp = await _client.SendAsync(changeReq);
            changeResp.StatusCode.Should().BeOneOf(System.Net.HttpStatusCode.OK, System.Net.HttpStatusCode.BadRequest);

            // Logout
            var logoutReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout")
            {
                Content = JsonContent.Create(new { refreshToken, logoutFromAllDevices = false }, options: _json)
            };
            logoutReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var logoutResp = await _client.SendAsync(logoutReq);
            logoutResp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Forgot password returns OK for existing user")]
        public async Task ForgotPassword_ExistingUser_Ok()
        {
            var email = $"sanity_{Guid.NewGuid():N}@example.com";
            await WithScope(async sp =>
            {
                var um = sp.GetRequiredService<UserManager<ApplicationUser>>();
                var user = new ApplicationUser
                {
                    UserName = email, Email = email, EmailConfirmed = true,
                    FirstName = "Sanity", LastName = "User", IsActive = true, CreatedAt = DateTime.UtcNow, UserTypeId = 2
                };
                var r = await um.CreateAsync(user, "Admin123!");
                r.Succeeded.Should().BeTrue();
            });

            var resp = await _client.PostAsJsonAsync("/api/auth/forgot-password", new { email }, _json);
            resp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Confirm email succeeds with valid token")]
        public async Task ConfirmEmail_HappyPath()
        {
            string userId = string.Empty; string token = string.Empty; string email = $"sanity_{Guid.NewGuid():N}@example.com";
            await WithScope(async sp =>
            {
                var um = sp.GetRequiredService<UserManager<ApplicationUser>>();
                var user = new ApplicationUser
                {
                    UserName = email, Email = email, EmailConfirmed = false,
                    FirstName = "Sanity", LastName = "User", IsActive = true, CreatedAt = DateTime.UtcNow, UserTypeId = 2
                };
                var r = await um.CreateAsync(user, "Admin123!");
                r.Succeeded.Should().BeTrue();
                userId = user.Id;
                token = await um.GenerateEmailConfirmationTokenAsync(user);
            });

            var resp = await _client.PostAsJsonAsync("/api/auth/confirm-email", new { userId, token }, _json);
            resp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            await WithScope(async sp =>
            {
                var um = sp.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await um.FindByIdAsync(userId);
                user!.EmailConfirmed.Should().BeTrue();
            });
        }

        [Fact(DisplayName = "Reset password with valid token updates password")]
        public async Task ResetPassword_HappyPath()
        {
            string email = $"sanity_{Guid.NewGuid():N}@example.com"; string token = string.Empty;
            await WithScope(async sp =>
            {
                var um = sp.GetRequiredService<UserManager<ApplicationUser>>();
                var user = new ApplicationUser
                {
                    UserName = email, Email = email, EmailConfirmed = true,
                    FirstName = "Sanity", LastName = "User", IsActive = true, CreatedAt = DateTime.UtcNow, UserTypeId = 2
                };
                (await um.CreateAsync(user, "Admin123!")).Succeeded.Should().BeTrue();
                token = await um.GeneratePasswordResetTokenAsync(user);
            });

            var newPwd = "Admin456!";
            var resp = await _client.PostAsJsonAsync("/api/auth/reset-password", new { email, token, newPassword = newPwd, confirmPassword = newPwd }, _json);
            resp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = newPwd }, _json);
            loginResp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
