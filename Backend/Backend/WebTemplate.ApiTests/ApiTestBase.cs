namespace WebTemplate.ApiTests
{
    using System.Net.Http.Headers;
    using System.Text.Json;
    using Xunit;

    /// <summary>
    /// Base class for API integration tests.
    /// Ensures factory is properly initialized before client creation.
    /// </summary>
    public abstract class ApiTestBase : IAsyncLifetime
    {
        protected TestWebAppFactory Factory { get; private set; } = default!;
        protected HttpClient Client { get; private set; } = default!;
        protected JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Initialize the test factory and client.
        /// Must be called before any tests run.
        /// </summary>
        public virtual async Task InitializeAsync()
        {
            Factory = new TestWebAppFactory();
            // Force initialization of the web app
            await Factory.InitializeAsync();
            
            // Create client AFTER factory is initialized
            Client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost/"),
                AllowAutoRedirect = false
            });
        }

        public virtual async Task DisposeAsync()
        {
            Client?.Dispose();
            Factory?.Dispose();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates an authenticated request with bearer token
        /// </summary>
        protected HttpRequestMessage CreateAuthenticatedRequest(HttpMethod method, string url, string? token = null)
        {
            var req = new HttpRequestMessage(method, url);
            if (token != null)
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return req;
        }

        /// <summary>
        /// Gets admin JWT token for authenticated requests
        /// </summary>
        protected async Task<string> GetAdminTokenAsync()
        {
            var loginPayload = new { email = "admin@WebTemplate.com", password = "Admin123!@#" };
            var loginResp = await Client.PostAsJsonAsync("/api/auth/login", loginPayload, JsonOptions);
            loginResp.EnsureSuccessStatusCode();

            var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            return loginJson.GetProperty("data").GetProperty("accessToken").GetString()!;
        }
    }
}
