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
        private bool _serverValidated = false;
        private static readonly object _validationLock = new();

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
        /// Validates that the backend server is running and responding.
        /// Lazy initialization - only checks once per test session.
        /// </summary>
        private void EnsureServerIsRunning()
        {
            if (_serverValidated)
                return;

            lock (_validationLock)
            {
                if (_serverValidated)
                    return;

                try
                {
                    var healthTask = Client.GetAsync("/health");
                    healthTask.Wait(TimeSpan.FromSeconds(2));
                    _serverValidated = true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"E2E tests require the backend server to be running.\n\n" +
                        $"Server URL configured: {Config.Server.BaseUrl}\n\n" +
                        $"To start the backend server, run:\n" +
                        $"  cd Backend/WebTemplate.API\n" +
                        $"  dotnet run\n\n" +
                        $"After the server is running and shows 'Now listening on: {Config.Server.BaseUrl}',\n" +
                        $"you can run E2E tests again.\n\n" +
                        $"Original error: {ex.GetBaseException().Message}",
                        ex);
                }
            }
        }

        /// <summary>
        /// Authenticates as admin and returns the access token
        /// </summary>
        protected async Task<string> LoginAsAdminAsync()
        {
            EnsureServerIsRunning();

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
        /// Registers a new user and returns userId, access token, and refresh token
        /// </summary>
        protected async Task<(string userId, string accessToken, string refreshToken)> RegisterUserAsync(string email, string password, string firstName = "Test", string lastName = "User")
        {
            EnsureServerIsRunning();

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

            // Get the data element - may be null if email confirmation required
            var dataElement = json.RootElement.GetProperty("data");
            
            if (dataElement.ValueKind == JsonValueKind.Null)
            {
                // Email confirmation is required - we can't auto-login
                throw new InvalidOperationException(
                    "Registration successful but email confirmation is required. " +
                    "E2E tests cannot proceed without confirmation token. " +
                    "Ensure 'RequireConfirmedEmail' is false in appsettings for E2E testing.");
            }

            var userId = dataElement.GetProperty("user").GetProperty("id").GetString()
                ?? throw new InvalidOperationException("Failed to get userId from register response");
            var accessToken = dataElement.GetProperty("accessToken").GetString()
                ?? throw new InvalidOperationException("Failed to get access token from register response");
            var refreshToken = dataElement.GetProperty("refreshToken").GetString()
                ?? throw new InvalidOperationException("Failed to get refresh token from register response");

            return (userId, accessToken, refreshToken);
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
