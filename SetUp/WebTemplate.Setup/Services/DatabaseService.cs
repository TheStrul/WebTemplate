using Microsoft.Data.SqlClient;
using System.Text;

namespace WebTemplate.Setup.Services;

/// <summary>
/// Service for database operations: testing connections and executing SQL scripts
/// </summary>
public class DatabaseService
{
    /// <summary>
    /// Tests database connection
    /// </summary>
    public async Task<(bool Success, string Message)> TestConnectionAsync(string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return (true, "Connection successful");
        }
        catch (SqlException ex)
        {
            return (false, $"SQL Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Connection failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a database exists
    /// </summary>
    public async Task<bool> DatabaseExistsAsync(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync();

            var checkCommand = new SqlCommand(
                $"SELECT database_id FROM sys.databases WHERE name = @dbName",
                connection
            );
            checkCommand.Parameters.AddWithValue("@dbName", databaseName);

            var result = await checkCommand.ExecuteScalarAsync();
            return result != null;
        }
        catch (Exception ex)
        {
            // If we can't connect or check, assume it doesn't exist
            return false;
        }
    }

    /// <summary>
    /// Creates a database
    /// Note: Caller should verify database doesn't already exist using DatabaseExistsAsync
    /// </summary>
    public async Task<(bool Success, string Message)> CreateDatabaseIfNotExistsAsync(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync();

            // Create database
            var createCommand = new SqlCommand(
                $"CREATE DATABASE [{databaseName}]",
                connection
            );
            await createCommand.ExecuteNonQueryAsync();

            return (true, $"Database '{databaseName}' created successfully");
        }
        catch (SqlException ex)
        {
            return (false, $"SQL Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error creating database: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a SQL initialization script
    /// </summary>
    public async Task<(bool Success, string Message)> ExecuteInitScriptAsync(
        string connectionString,
        string scriptPath)
    {
        try
        {
            if (!File.Exists(scriptPath))
            {
                return (false, $"Script file not found: {scriptPath}");
            }

            var script = await File.ReadAllTextAsync(scriptPath);
            var batches = SplitSqlBatches(script);

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var successCount = 0;
            var errorMessages = new List<string>();

            foreach (var batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch))
                    continue;

                try
                {
                    using var command = new SqlCommand(batch, connection);
                    command.CommandTimeout = 300; // 5 minutes for long-running scripts
                    await command.ExecuteNonQueryAsync();
                    successCount++;
                }
                catch (SqlException ex)
                {
                    errorMessages.Add($"Batch {successCount + 1} failed: {ex.Message}");
                    // Continue with other batches
                }
            }

            if (errorMessages.Any())
            {
                return (false, $"Script executed with errors:\n{string.Join("\n", errorMessages)}");
            }

            return (true, $"Script executed successfully. {successCount} batches completed.");
        }
        catch (SqlException ex)
        {
            return (false, $"SQL Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error executing script: {ex.Message}");
        }
    }

    /// <summary>
    /// Splits SQL script into batches (separated by GO statements)
    /// </summary>
    private List<string> SplitSqlBatches(string script)
    {
        var batches = new List<string>();
        var currentBatch = new StringBuilder();

        using var reader = new StringReader(script);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            // Check if line is a GO statement (case-insensitive, whole line)
            var trimmedLine = line.Trim();
            if (trimmedLine.Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                if (currentBatch.Length > 0)
                {
                    batches.Add(currentBatch.ToString());
                    currentBatch.Clear();
                }
            }
            else
            {
                currentBatch.AppendLine(line);
            }
        }

        // Add final batch if any
        if (currentBatch.Length > 0)
        {
            batches.Add(currentBatch.ToString());
        }

        return batches;
    }

    /// <summary>
    /// Extracts database name from connection string
    /// </summary>
    public string? GetDatabaseName(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return builder.InitialCatalog;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validates connection string format
    /// </summary>
    public (bool IsValid, string Message) ValidateConnectionString(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            if (string.IsNullOrWhiteSpace(builder.DataSource))
            {
                return (false, "Connection string must specify a DataSource (server)");
            }

            if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            {
                return (false, "Connection string must specify an InitialCatalog (database name)");
            }

            return (true, "Connection string is valid");
        }
        catch (Exception ex)
        {
            return (false, $"Invalid connection string: {ex.Message}");
        }
    }
}
