namespace WebTemplate.E2ETests
{
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;

    /// <summary>
    /// Base class for E2E tests with helper methods for authentication and HTTP calls
    /// </summary>
    public abstract class E2ETestBase : IDisposable
    {
        protected readonly HttpClient Client;
        protected readonly JsonSerializerOptions JsonOptions;

        protected E2ETestBase()
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri(E2ETestConfig.BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            JsonOptions = E2ETestConfig.JsonOptions;
        }

        /// <summary>
        /// Authenticates as admin and returns the access token
        /// </summary>
        protected async Task<string> LoginAsAdminAsync()
        {
            var loginRequest = new
            {
                email = E2ETestConfig.AdminEmail,
                password = E2ETestConfig.AdminPassword
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
                firstName,
                lastName
            };

            var response = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            var userId = json.RootElement.GetProperty("data").GetProperty("userId").GetString()
                ?? throw new InvalidOperationException("Failed to get userId from register response");
            var token = json.RootElement.GetProperty("data").GetProperty("accessToken").GetString()
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
