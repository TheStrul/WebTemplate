namespace WebTemplate.E2ETests
{
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.Json;

    /// <summary>
    /// Detects if the WebTemplate.API server is running and determines the correct connection URL.
    /// Automatically checks common ports and validates server responsiveness.
    /// </summary>
    public static class ServerDetector
    {
    /// <summary>
    /// Detects the running server URL from launchSettings.json only.
    /// NO FALLBACKS - fails fast if configuration is missing or invalid.
    /// </summary>
    public static string DetectServerUrl()
    {
        // Read from launchSettings.json - NO FALLBACKS
        var launchSettingsUrls = ReadLaunchSettingsUrls();

        if (launchSettingsUrls.Count == 0)
        {
            throw new InvalidOperationException(
                "Cannot detect server URL: launchSettings.json not found or has no applicationUrl configured.\n\n" +
                "Ensure Backend/WebTemplate.API/Properties/launchSettings.json exists with applicationUrl configured.\n\n" +
                "NO FALLBACKS - explicit configuration required!");
        }

        foreach (var url in launchSettingsUrls)
        {
            if (IsServerResponsive(url))
                return url;
        }

        // Nothing found - FAIL FAST with clear error
        throw new InvalidOperationException(
            "WebTemplate.API server is not running or not responding.\n\n" +
            "To run E2E tests, please start the server first:\n" +
            "  cd Backend/WebTemplate.API\n" +
            "  dotnet run\n\n" +
            $"Checked launchSettings.json URLs: {string.Join(", ", launchSettingsUrls)}");
    }        /// <summary>
        /// Checks if the server is responsive by attempting to connect to the health endpoint.
        /// </summary>
        private static bool IsServerResponsive(string baseUrl)
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

                // First, check if the port is listening
                var uri = new Uri(baseUrl);
                if (!IsPortListening(uri.Host, uri.Port))
                    return false;

                // Then try the health endpoint
                var response = client.GetAsync($"{baseUrl}/health").GetAwaiter().GetResult();
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a port is listening without making an HTTP request.
        /// </summary>
        private static bool IsPortListening(string host, int port)
        {
            try
            {
                using var tcpClient = new TcpClient();
                var result = tcpClient.BeginConnect(host, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));

                if (success)
                {
                    tcpClient.EndConnect(result);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Finds launchSettings.json by searching up the directory tree.
        /// </summary>
        private static string? FindLaunchSettings(string startDir)
        {
            var dir = new DirectoryInfo(startDir);

            // Search up to 5 levels
            for (int i = 0; i < 5 && dir != null; i++)
            {
                // Look for WebTemplate.API/Properties/launchSettings.json
                var apiPath = Path.Combine(dir.FullName, "WebTemplate.API", "Properties", "launchSettings.json");
                if (File.Exists(apiPath))
                    return apiPath;

                // Also check sibling directory (when running from E2ETests folder)
                var siblingPath = Path.Combine(dir.FullName, "..", "WebTemplate.API", "Properties", "launchSettings.json");
                var fullSiblingPath = Path.GetFullPath(siblingPath);
                if (File.Exists(fullSiblingPath))
                    return fullSiblingPath;

                dir = dir.Parent;
            }

            return null;
        }

        /// <summary>
        /// Reads application URLs from launchSettings.json in the API project.
        /// </summary>
        private static List<string> ReadLaunchSettingsUrls()
        {
            var urls = new List<string>();

            try
            {
                // Find launchSettings.json by searching up the directory tree
                var currentDir = Directory.GetCurrentDirectory();
                var launchSettingsPath = FindLaunchSettings(currentDir);

                if (string.IsNullOrEmpty(launchSettingsPath))
                {
                    return urls;
                }

                var json = File.ReadAllText(launchSettingsPath);
                using var doc = JsonDocument.Parse(json);

                var root = doc.RootElement;
                if (root.TryGetProperty("profiles", out var profiles))
                {
                    foreach (var profile in profiles.EnumerateObject())
                    {
                        if (profile.Value.TryGetProperty("applicationUrl", out var appUrl))
                        {
                            var urlString = appUrl.GetString();
                            if (!string.IsNullOrWhiteSpace(urlString))
                            {
                                // Handle multiple URLs separated by semicolon
                                var splitUrls = urlString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                                urls.AddRange(splitUrls.Select(u => u.Trim()));
                            }
                        }
                    }
                }

                // Also check iisSettings
                if (root.TryGetProperty("iisSettings", out var iisSettings))
                {
                    if (iisSettings.TryGetProperty("iisExpress", out var iisExpress))
                    {
                        if (iisExpress.TryGetProperty("applicationUrl", out var iisUrl))
                        {
                            var urlString = iisUrl.GetString();
                            if (!string.IsNullOrWhiteSpace(urlString))
                                urls.Add(urlString);
                        }
                    }
                }
            }
            catch
            {
                // If we can't read launchSettings, just return empty list
            }

            return urls;
        }

        /// <summary>
        /// Gets server information including URL, version, and health status.
        /// </summary>
        public static ServerInfo GetServerInfo(string baseUrl)
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

            var info = new ServerInfo { BaseUrl = baseUrl };

            try
            {
                // Check health endpoint
                var healthResponse = client.GetAsync($"{baseUrl}/health").GetAwaiter().GetResult();
                info.IsHealthy = healthResponse.IsSuccessStatusCode;

                if (info.IsHealthy)
                {
                    var healthContent = healthResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    info.HealthStatus = healthContent;
                }
            }
            catch (Exception ex)
            {
                info.IsHealthy = false;
                info.Error = ex.Message;
            }

            return info;
        }
    }

    /// <summary>
    /// Information about the detected server.
    /// </summary>
    public class ServerInfo
    {
        public string BaseUrl { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public string? HealthStatus { get; set; }
        public string? Error { get; set; }
    }
}
