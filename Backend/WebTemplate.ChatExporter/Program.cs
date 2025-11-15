using System.Data.SQLite;
using Dapper;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace WebTemplate.ChatExporter;

/// <summary>
/// Exports GitHub Copilot chat history from VS local storage to readable markdown files
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 GitHub Copilot Chat Exporter");
        Console.WriteLine("================================\n");

        var exporter = new CopilotChatExporter();
        
        try
        {
            var result = await exporter.ExportAllChatsAsync();
            
            if (result.Success)
            {
                Console.WriteLine($"\n✅ Export completed successfully!");
                Console.WriteLine($"   📁 Location: {result.OutputDirectory}");
                Console.WriteLine($"   📄 Files created: {result.FilesCreated}");
                Console.WriteLine($"   💬 Chats exported: {result.ChatsExported}");
            }
            else
            {
                Console.WriteLine($"\n❌ Export failed: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.WriteLine($"   {ex.StackTrace}");
        }
    }
}

/// <summary>
/// Handles exporting Copilot chat history to markdown
/// </summary>
public class CopilotChatExporter
{
    private readonly string _vsAppDataPath;
    private readonly string _outputDirectory;

    public CopilotChatExporter()
    {
        _vsAppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft", "VisualStudio"
        );

        // Use absolute path to SESSIONS folder in the WebTemplate repo
        var solutionRoot = FindSolutionRoot();
        _outputDirectory = Path.Combine(solutionRoot, "SESSIONS", "copilot_exports");
    }

    private string FindSolutionRoot()
    {
        // Start from current directory and walk up looking for .git or .sln
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        
        while (currentDir != null)
        {
            if (File.Exists(Path.Combine(currentDir.FullName, "WebTemplate.sln")) ||
                Directory.Exists(Path.Combine(currentDir.FullName, ".git")))
            {
                return currentDir.FullName;
            }
            currentDir = currentDir.Parent;
        }

        // Fallback to a known location
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "source", "repos", "GitHubLocal", "Customers", "WebTemplate"
        );
    }

    public async Task<ExportResult> ExportAllChatsAsync()
    {
        Console.WriteLine($"📍 Scanning: {_vsAppDataPath}\n");

        var dbFiles = FindCopilotDatabases();
        
        if (dbFiles.Count == 0)
        {
            return new ExportResult
            {
                Success = false,
                Message = "No Copilot chat databases found"
            };
        }

        Console.WriteLine($"Found {dbFiles.Count} Copilot database(s):");
        foreach (var db in dbFiles)
        {
            Console.WriteLine($"  📂 {db}");
        }
        Console.WriteLine();

        Directory.CreateDirectory(_outputDirectory);

        int totalChats = 0;
        int totalFiles = 0;

        foreach (var dbPath in dbFiles)
        {
            try
            {
                Console.WriteLine($"Processing: {Path.GetFileName(Path.GetDirectoryName(dbPath))}");
                var (chats, files) = await ExportFromDatabaseAsync(dbPath);
                totalChats += chats;
                totalFiles += files;
                Console.WriteLine($"  ✅ {chats} chat(s), {files} file(s)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠️  Error: {ex.Message}\n");
            }
        }

        return new ExportResult
        {
            Success = true,
            OutputDirectory = _outputDirectory,
            ChatsExported = totalChats,
            FilesCreated = totalFiles,
            Message = "Export completed successfully"
        };
    }

    private List<string> FindCopilotDatabases()
    {
        var dbFiles = new List<string>();

        try
        {
            // Main Copilot folder
            var mainCopilotDir = Path.Combine(_vsAppDataPath, "Copilot");
            if (Directory.Exists(mainCopilotDir))
            {
                var files = Directory.GetFiles(mainCopilotDir, "*.db", SearchOption.AllDirectories);
                dbFiles.AddRange(files);
            }

            // VS instance-specific folders (17.0_xxxxx pattern)
            var vsInstanceDirs = Directory.GetDirectories(_vsAppDataPath, "17.0_*");
            foreach (var instanceDir in vsInstanceDirs)
            {
                var copilotDir = Path.Combine(instanceDir, "copilot");
                if (Directory.Exists(copilotDir))
                {
                    var files = Directory.GetFiles(copilotDir, "*.db", SearchOption.AllDirectories);
                    dbFiles.AddRange(files);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Error scanning directories: {ex.Message}");
        }

        return dbFiles.Distinct().ToList();
    }

    private async Task<(int chats, int files)> ExportFromDatabaseAsync(string dbPath)
    {
        var connectionString = $"Data Source={dbPath};Version=3;";
        
        using (var connection = new SQLiteConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                
                // Query all tables to understand structure
                var tables = await GetTableNamesAsync(connection);
                Console.WriteLine($"     Tables: {string.Join(", ", tables)}");

                // Export chat data
                var (chats, files) = await ExportChatsFromConnectionAsync(connection, dbPath);
                return (chats, files);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"     Error querying database: {ex.Message}");
                return (0, 0);
            }
        }
    }

    private async Task<List<string>> GetTableNamesAsync(SQLiteConnection connection)
    {
        var query = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
        var tables = (await connection.QueryAsync<string>(query)).ToList();
        return tables;
    }

    private async Task<(int chats, int files)> ExportChatsFromConnectionAsync(SQLiteConnection connection, string dbPath)
    {
        try
        {
            // Try to get conversation data - structure may vary
            var query = @"
                SELECT * FROM sqlite_master 
                WHERE type='table' AND name NOT LIKE 'sqlite_%'
                LIMIT 10
            ";
            
            var result = await connection.QueryAsync(query);
            var tableInfo = result.ToList();

            if (!tableInfo.Any())
            {
                return await ExportGenericChatDataAsync(connection, dbPath);
            }

            return await ExportSpecificChatDataAsync(connection, dbPath, tableInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"     Error: {ex.Message}");
            return (0, 0);
        }
    }

    private async Task<(int chats, int files)> ExportSpecificChatDataAsync(SQLiteConnection connection, string dbPath, IEnumerable<dynamic> tableInfo)
    {
        int chatsExported = 0;
        int filesCreated = 0;

        try
        {
            foreach (var table in tableInfo)
            {
                var tableName = table.name;

                try
                {
                    // For known chat tables, export in a structured format
                    if (tableName.Contains("Conversations"))
                    {
                        var query = $"SELECT * FROM [{tableName}]";
                        var records = (await connection.QueryAsync(query)).ToList();

                        if (records.Count > 0)
                        {
                            var fileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{tableName}.md";
                            var filePath = Path.Combine(_outputDirectory, fileName);

                            var markdown = new StringBuilder();
                            markdown.AppendLine($"# Copilot Chat Export - {tableName}");
                            markdown.AppendLine($"**Exported:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                            markdown.AppendLine($"**Source:** {Path.GetFileName(dbPath)}");
                            markdown.AppendLine($"**Records:** {records.Count}");
                            markdown.AppendLine();

                            foreach (var record in records)
                            {
                                markdown.AppendLine($"## Chat ID: {record.Id}");
                                markdown.AppendLine($"**Prompt:** {record.Prompt}");
                                markdown.AppendLine($"**Response:** {record.Response}");
                                markdown.AppendLine($"**Timestamp:** {record.Timestamp}");
                                markdown.AppendLine();
                            }

                            await File.WriteAllTextAsync(filePath, markdown.ToString());
                            filesCreated++;
                            chatsExported++;

                            Console.WriteLine($"     ✓ {tableName}: {records.Count} record(s)");
                        }
                    }
                    else
                    {
                        // Generic export for other tables
                        var query = $"SELECT * FROM [{tableName}] LIMIT 1000";
                        var records = (await connection.QueryAsync(query)).ToList();

                        if (records.Count > 0)
                        {
                            var fileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{tableName}.md";
                            var filePath = Path.Combine(_outputDirectory, fileName);

                            var markdown = new StringBuilder();
                            markdown.AppendLine($"# Copilot Chat Export - {tableName}");
                            markdown.AppendLine($"**Exported:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                            markdown.AppendLine($"**Source:** {Path.GetFileName(dbPath)}");
                            markdown.AppendLine($"**Records:** {records.Count}");
                            markdown.AppendLine();

                            markdown.AppendLine("## Data");
                            markdown.AppendLine("```json");
                            
                            foreach (var record in records)
                            {
                                var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
                                markdown.AppendLine(json);
                                markdown.AppendLine();
                            }

                            markdown.AppendLine("```");

                            await File.WriteAllTextAsync(filePath, markdown.ToString());
                            filesCreated++;
                            chatsExported++;

                            Console.WriteLine($"     ✓ {tableName}: {records.Count} record(s)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"     ⚠️  {tableName}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"     Error: {ex.Message}");
        }

        return (chatsExported, filesCreated);
    }

    private async Task<(int chats, int files)> ExportGenericChatDataAsync(SQLiteConnection connection, string dbPath)
    {
        int chatsExported = 0;
        int filesCreated = 0;

        try
        {
            // Get all tables
            var tables = await GetTableNamesAsync(connection);
            Console.WriteLine($"     Available tables: {string.Join(", ", tables)}");

            foreach (var table in tables)
            {
                try
                {
                    // Get column info first
                    var columnQuery = $"PRAGMA table_info([{table}])";
                    var columns = (await connection.QueryAsync(columnQuery)).ToList();

                    if (columns.Count == 0)
                        continue;

                    // Try to get data from each table
                    var query = $"SELECT * FROM [{table}] LIMIT 1000";
                    var records = (await connection.QueryAsync(query)).ToList();

                    if (records.Count > 0)
                    {
                        // Export this table's data
                        var fileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{table}.md";
                        var filePath = Path.Combine(_outputDirectory, fileName);

                        var markdown = new StringBuilder();
                        markdown.AppendLine($"# Copilot Chat Export");
                        markdown.AppendLine();
                        markdown.AppendLine($"| Property | Value |");
                        markdown.AppendLine($"|----------|-------|");
                        markdown.AppendLine($"| Exported | {DateTime.Now:yyyy-MM-dd HH:mm:ss} |");
                        markdown.AppendLine($"| Source | {Path.GetFileName(dbPath)} |");
                        markdown.AppendLine($"| Table | {table} |");
                        markdown.AppendLine($"| Records | {records.Count} |");
                        markdown.AppendLine();

                        markdown.AppendLine("## Raw Data");
                        markdown.AppendLine();
                        markdown.AppendLine("```json");
                        
                        foreach (var record in records)
                        {
                            var json = JsonSerializer.Serialize(record, new JsonSerializerOptions 
                            { 
                                WriteIndented = true,
                                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                            });
                            markdown.AppendLine(json);
                            markdown.AppendLine();
                        }

                        markdown.AppendLine("```");

                        await File.WriteAllTextAsync(filePath, markdown.ToString());
                        filesCreated++;
                        chatsExported++;

                        Console.WriteLine($"     ✓ Exported {table}: {records.Count} record(s) → {fileName}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"     ⚠️  {table}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"     Error: {ex.Message}");
        }

        return (chatsExported, filesCreated);
    }
}

/// <summary>
/// Result of the export operation
/// </summary>
public class ExportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public int ChatsExported { get; set; }
    public int FilesCreated { get; set; }
}
