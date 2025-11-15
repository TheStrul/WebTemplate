namespace WebTemplate.E2ETests
{
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;
    using WebTemplate.E2ETests.Configuration;

    /// <summary>
    /// Base class for E2E tests with helper methods for authentication and HTTP calls.
    /// Uses hierarchical configuration singleton pattern with NO FALLBACKS.
    /// </summary>
    public abstract class E2ETestBase : IDisposable
    {
        protected readonly HttpClient Client;
        protected readonly JsonSerializerOptions JsonOptions;
        protected readonly ITestConfiguration Config;

        protected E2ETestBase()
        {
            Config = TestConfiguration.Instance;

            Client = new HttpClient
            {
                BaseAddress = new Uri(Config.Server.BaseUrl),
                Timeout = TimeSpan.FromSeconds(Config.Execution.RequestTimeoutSeconds)
            };

            JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Authenticates as admin and returns the access token
        /// </summary>
        protected async Task<string> LoginAsAdminAsync()
        {
            var loginRequest = new
            {
                email = Config.Admin.Email,
                password = Config.Admin.Password
            };

            var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            return json.RootElement.GetProperty("data").GetProperty("accessToken").GetString()
                ?? throw new InvalidOperationException("Failed to get access token from login response");
        }

        /// <summary>
        /// Registers a new user and returns userId and access token
        /// </summary>
        protected async Task<(string userId, string token)> RegisterUserAsync(string email, string password, string firstName = "Test", string lastName = "User")
        {
            var registerRequest = new
            {
                email,
                password,
                confirmPassword = password,
                firstName,
                lastName,
                acceptTerms = true,
                acceptPrivacyPolicy = true
            };

            var response = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            // Check if data is null (might be due to email confirmation requirement)
            var dataElement = json.RootElement.GetProperty("data");
            if (dataElement.ValueKind == JsonValueKind.Null)
            {
                // For email confirmation flow, we need to handle it differently
                // For now, extract the userId from somewhere else or throw a clear error
                throw new InvalidOperationException("Registration returned no data - email confirmation may be required");
            }

            var userId = dataElement.GetProperty("userId").GetString()
                ?? throw new InvalidOperationException("Failed to get userId from register response");
            var token = dataElement.GetProperty("accessToken").GetString()
                ?? throw new InvalidOperationException("Failed to get access token from register response");

            return (userId, token);
        }

        /// <summary>
        /// Sets the authorization header with the provided token
        /// </summary>
        protected void SetAuthToken(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Clears the authorization header
        /// </summary>
        protected void ClearAuthToken()
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }

        public void Dispose()
        {
            Client?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
