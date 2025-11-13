namespace WebTemplate.Core.Common;

/// <summary>
/// Represents an error with structured information for logging and telemetry.
/// Designed to work seamlessly with Azure Application Insights.
/// </summary>
public sealed record Error
{
    /// <summary>
    /// Unique error code/identifier (e.g., "USER.NOT_FOUND", "AUTH.INVALID_TOKEN")
    /// This appears as a custom dimension in Application Insights
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Error type/category for classification and filtering
    /// </summary>
    public ErrorType Type { get; init; }

    /// <summary>
    /// Human-readable error description
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Optional inner exception for technical details
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Additional structured metadata for debugging and telemetry
    /// Maps directly to Application Insights custom dimensions
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; }

    /// <summary>
    /// Creates a new Error instance
    /// </summary>
    public Error(string code, ErrorType type, string description, Exception? exception = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be null or empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Error description cannot be null or empty.", nameof(description));

        Code = code;
        Type = type;
        Description = description;
        Exception = exception;
        Metadata = new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a new Error instance with metadata
    /// </summary>
    public Error(string code, ErrorType type, string description, Dictionary<string, object> metadata, Exception? exception = null)
        : this(code, type, description, exception)
    {
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Adds metadata to the error for enhanced telemetry
    /// </summary>
    public Error WithMetadata(string key, object value)
    {
        var newMetadata = new Dictionary<string, object>(Metadata) { [key] = value };
        return this with { Metadata = newMetadata };
    }

    /// <summary>
    /// Creates an error from an exception
    /// </summary>
    public static Error FromException(Exception exception, string? code = null)
    {
        return new Error(
            code ?? "EXCEPTION.UNHANDLED",
            ErrorType.Failure,
            exception.Message,
            exception
        );
    }

    public override string ToString() => $"[{Type}] {Code}: {Description}";
}

/// <summary>
/// Error type enumeration for categorizing errors.
/// This helps with filtering and querying in Application Insights.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Validation error (400-level, client error)
    /// </summary>
    Validation,

    /// <summary>
    /// Resource not found (404)
    /// </summary>
    NotFound,

    /// <summary>
    /// Authentication failed (401)
    /// </summary>
    Unauthorized,

    /// <summary>
    /// Authorization failed - insufficient permissions (403)
    /// </summary>
    Forbidden,

    /// <summary>
    /// Conflict with existing resource (409)
    /// </summary>
    Conflict,

    /// <summary>
    /// Business rule violation
    /// </summary>
    BusinessRule,

    /// <summary>
    /// General failure (500-level, server error)
    /// </summary>
    Failure,

    /// <summary>
    /// External service/dependency failure
    /// </summary>
    External,

    /// <summary>
    /// Configuration error
    /// </summary>
    Configuration,

    /// <summary>
    /// Database/data access error
    /// </summary>
    Database,

    /// <summary>
    /// Network/connectivity error
    /// </summary>
    Network,

    /// <summary>
    /// Timeout error
    /// </summary>
    Timeout,

    /// <summary>
    /// Rate limit exceeded
    /// </summary>
    RateLimit
}
