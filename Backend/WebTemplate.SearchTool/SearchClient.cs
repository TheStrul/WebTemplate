namespace WebTemplate.SearchTool;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;

/// <summary>
/// Simple web search tool for knowledge base population.
/// Uses Tavily API (free tier: ai-powered search optimized for AI agents)
/// </summary>
public class SearchClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string ApiEndpoint = "https://api.tavily.com/search";

    public SearchClient(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required and must not be empty.", nameof(apiKey));

        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Searches for information and returns markdown-formatted results
    /// </summary>
    public async Task<string> SearchAsync(string query, int maxResults = 5)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Search query is required and must not be empty.", nameof(query));

        try
        {
            var request = new
            {
                api_key = _apiKey,
                query = query,
                max_results = maxResults,
                include_answer = true,
                include_raw_content = false,
                topic = "general"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(ApiEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Search API request failed with status {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return FormatResultsAsMarkdown(query, result);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                "Failed to connect to search API. Verify internet connection and API endpoint.", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "Failed to parse search API response.", ex);
        }
    }

    /// <summary>
    /// Formats search results as markdown for easy integration into documentation
    /// </summary>
    private static string FormatResultsAsMarkdown(string query, JsonElement result)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine($"# Search Results: {query}");
        markdown.AppendLine();

        try
        {
            // Include answer if available
            if (result.TryGetProperty("answer", out var answerElement) && 
                !string.IsNullOrWhiteSpace(answerElement.GetString()))
            {
                markdown.AppendLine("## Quick Answer");
                markdown.AppendLine();
                markdown.AppendLine(answerElement.GetString());
                markdown.AppendLine();
            }

            // Add search results
            if (result.TryGetProperty("results", out var resultsElement))
            {
                markdown.AppendLine("## Sources");
                markdown.AppendLine();

                int index = 1;
                foreach (var resultItem in resultsElement.EnumerateArray())
                {
                    var title = resultItem.TryGetProperty("title", out var titleElement) 
                        ? titleElement.GetString() 
                        : "Untitled";
                    
                    var url = resultItem.TryGetProperty("url", out var urlElement) 
                        ? urlElement.GetString() 
                        : "";
                    
                    var content = resultItem.TryGetProperty("content", out var contentElement) 
                        ? contentElement.GetString() 
                        : "";

                    markdown.AppendLine($"### {index}. [{title}]({url})");
                    markdown.AppendLine();
                    
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        markdown.AppendLine(content);
                        markdown.AppendLine();
                    }

                    index++;
                }
            }
        }
        catch (Exception ex)
        {
            markdown.AppendLine($"**Error formatting results:** {ex.Message}");
        }

        markdown.AppendLine("---");
        markdown.AppendLine($"*Search performed on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*");

        return markdown.ToString();
    }
}
