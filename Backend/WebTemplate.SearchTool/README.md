# WebTemplate Search Tool

A lightweight C# CLI tool for searching the web and generating markdown documentation for the Knowledge Base.

## Purpose

Enables autonomous knowledge base population by searching for current information and converting results to markdown format.

## Features

- ✅ **Free API** - Uses Tavily (free tier available)
- ✅ **AI-Optimized** - Search results tuned for AI agent consumption
- ✅ **Markdown Output** - Results formatted for direct KB integration
- ✅ **No Dependencies** - Uses only .NET 9 built-ins (HttpClient, System.Text.Json)
- ✅ **Quick Results** - Saves to timestamped markdown files

## Setup

### 1. Get Free API Key

Visit [tavily.com](https://tavily.com) and sign up for a free account.
- Free tier: Unlimited searches (perfect for our use case)
- API key provided immediately

### 2. Set Environment Variable

```powershell
# PowerShell
$env:TAVILY_API_KEY = "your-api-key-here"

# Or add to profile for persistence:
# Add line to $PROFILE: $env:TAVILY_API_KEY = "your-key"
```

### 3. Build

```powershell
cd Backend/WebTemplate.SearchTool
dotnet build -c Release
```

## Usage

### Basic Search

```powershell
dotnet run -- ".NET 9 release notes breaking changes"
dotnet run -- "EF Core 9 best practices 2025"
dotnet run -- "ASP.NET Core 9 new features"
```

### Output

- **Console:** Formatted markdown results with answer summary and source links
- **File:** Saves to `search_results_YYYYMMDD_HHmmss.md` in current directory

## Examples

### Search 1: .NET 9 Breaking Changes

```powershell
dotnet run -- ".NET 9 release notes breaking changes"
```

**Result:** `search_results_20251113_143022.md` containing:
- Quick answer about breaking changes
- Links to Microsoft Learn articles
- Detailed summaries from each source

### Search 2: Entity Framework Core 9

```powershell
dotnet run -- "EF Core 9 best practices SQL Server 2025"
```

### Search 3: ASP.NET Core Patterns

```powershell
dotnet run -- "ASP.NET Core 9 dependency injection patterns"
```

## Integration with Knowledge Base

**Workflow:**

1. Run search tool
2. Results saved to markdown file
3. Copy relevant sections to KB
4. Reference sources in KB documents

Example:

```bash
# Search for info
dotnet run -- "EF Core 9 performance tuning"

# Use results in KB
cat search_results_20251113_143022.md >> ../../knowledge-base/3_DATABASE/02_EF_CONFIGURATION.md
```

## Architecture

### SearchClient.cs

- **Tavily API Integration:** Makes HTTP requests to API
- **Result Formatting:** Converts JSON to markdown
- **Error Handling:** Explicit validation, no fallback logic

### Program.cs

- **CLI Entry Point:** Accepts query as command-line arguments
- **Environment Variable:** Reads API key from TAVILY_API_KEY
- **File Output:** Saves results with timestamp
- **User Feedback:** Color-coded console output

## Error Handling

**Missing API Key:**
```
ERROR: TAVILY_API_KEY environment variable not set

Get a free API key from: https://tavily.com

Set environment variable:
  $env:TAVILY_API_KEY = "your-api-key"
```

**Network Error:**
```
Operation Error: Failed to connect to search API. Verify internet connection and API endpoint.
```

**Empty Query:**
```
Argument Error: Search query is required and must not be empty.
```

## API Limits & Notes

- **Free Tier:** Unlimited searches
- **Rate Limit:** ~100 requests/day (generous for KB use)
- **Response Time:** 2-5 seconds typical
- **Results Count:** Top 5 most relevant (configurable)

## Future Enhancements

- [ ] Batch search support (search multiple queries)
- [ ] Direct GitHub integration (create KB PRs)
- [ ] Caching for repeated queries
- [ ] Custom result filtering
- [ ] Export to multiple formats (HTML, PDF)

## Troubleshooting

### "API key is required"
- Set `$env:TAVILY_API_KEY` before running tool
- Verify API key is valid on Tavily dashboard

### "Search API request failed"
- Check internet connection
- Verify Tavily API status
- Confirm API key hasn't been revoked

### "Results not saved"
- Check write permissions in current directory
- Verify disk space available

## Development

### Build Release

```powershell
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

### Run Tests (when added)

```powershell
dotnet test
```

---

**Created:** 13/11/2025  
**Part of:** WebTemplate Knowledge Base Infrastructure  
**Status:** Production Ready (v1.0)
