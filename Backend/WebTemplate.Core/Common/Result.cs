namespace WebTemplate.Core.Common;

/// <summary>
/// Represents the outcome of an operation that can succeed or fail.
/// This is the non-generic version for operations that don't return data.
/// Works seamlessly with Azure Application Insights structured logging.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the operation succeeded
    /// </summary>
    public bool IsSuccess { get; protected init; }

    /// <summary>
    /// Indicates whether the operation failed
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Error information when operation fails
    /// </summary>
    public Error? Error { get; protected init; }

    /// <summary>
    /// Collection of errors for operations that can fail in multiple ways
    /// </summary>
    public IReadOnlyList<Error> Errors { get; protected init; } = Array.Empty<Error>();

    /// <summary>
    /// Additional context metadata for logging and telemetry
    /// This populates Azure Application Insights custom dimensions
    /// </summary>
    public Dictionary<string, object> Context { get; protected init; } = new();

    /// <summary>
    /// Protected constructor to enforce factory methods
    /// </summary>
    protected Result(bool isSuccess, Error? error = null, IEnumerable<Error>? errors = null)
    {
        if (isSuccess && (error != null || (errors != null && errors.Any())))
            throw new InvalidOperationException("A successful result cannot contain errors.");

        if (!isSuccess && error == null && (errors == null || !errors.Any()))
            throw new InvalidOperationException("A failed result must contain at least one error.");

        IsSuccess = isSuccess;
        Error = error;

        // Build Errors collection: include both single error and errors collection
        var errorList = new List<Error>();
        if (error != null)
            errorList.Add(error);
        if (errors != null)
            errorList.AddRange(errors);

        Errors = errorList;
    }

    #region Factory Methods

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a successful result with context metadata
    /// </summary>
    public static Result Success(Dictionary<string, object> context)
    {
        var result = new Result(true) { Context = context };
        return result;
    }

    /// <summary>
    /// Creates a failed result with a single error
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with a single error and context
    /// </summary>
    public static Result Failure(Error error, Dictionary<string, object> context)
    {
        var result = new Result(false, error) { Context = context };
        return result;
    }

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public static Result Failure(IEnumerable<Error> errors) => new(false, errors: errors);

    /// <summary>
    /// Creates a failed result with multiple errors and context
    /// </summary>
    public static Result Failure(IEnumerable<Error> errors, Dictionary<string, object> context)
    {
        var result = new Result(false, errors: errors) { Context = context };
        return result;
    }

    #endregion

    #region Implicit Conversions

    /// <summary>
    /// Allows implicit conversion from Error to Result for cleaner code
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);

    #endregion

    /// <summary>
    /// Adds context metadata for telemetry and debugging
    /// </summary>
    public Result WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple context entries
    /// </summary>
    public Result WithContext(Dictionary<string, object> contextEntries)
    {
        foreach (var entry in contextEntries)
        {
            Context[entry.Key] = entry.Value;
        }
        return this;
    }
}

/// <summary>
/// Represents the outcome of an operation that returns data on success.
/// Generic version for operations that return a value.
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// The data returned by a successful operation
    /// </summary>
    public T? Value { get; private init; }

    /// <summary>
    /// Protected constructor to enforce factory methods
    /// </summary>
    private Result(bool isSuccess, T? value = default, Error? error = null, IEnumerable<Error>? errors = null)
        : base(isSuccess, error, errors)
    {
        if (isSuccess && EqualityComparer<T>.Default.Equals(value, default))
            throw new InvalidOperationException("A successful result must contain a value.");

        Value = value;
    }

    #region Factory Methods

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static Result<T> Success(T value) => new(true, value);

    /// <summary>
    /// Creates a successful result with data and context
    /// </summary>
    public static Result<T> Success(T value, Dictionary<string, object> context)
    {
        var result = new Result<T>(true, value) { Context = context };
        return result;
    }

    /// <summary>
    /// Creates a failed result with a single error
    /// </summary>
    public new static Result<T> Failure(Error error) => new(false, default, error);

    /// <summary>
    /// Creates a failed result with a single error and context
    /// </summary>
    public new static Result<T> Failure(Error error, Dictionary<string, object> context)
    {
        var result = new Result<T>(false, default, error) { Context = context };
        return result;
    }

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public new static Result<T> Failure(IEnumerable<Error> errors) => new(false, default, errors: errors);

    /// <summary>
    /// Creates a failed result with multiple errors and context
    /// </summary>
    public new static Result<T> Failure(IEnumerable<Error> errors, Dictionary<string, object> context)
    {
        var result = new Result<T>(false, default, errors: errors) { Context = context };
        return result;
    }

    #endregion

    #region Implicit Conversions

    /// <summary>
    /// Allows implicit conversion from T to Result<T> for cleaner code
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Allows implicit conversion from Error to Result<T> for cleaner code
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);

    #endregion

    /// <summary>
    /// Adds context metadata for telemetry and debugging
    /// </summary>
    public new Result<T> WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple context entries
    /// </summary>
    public new Result<T> WithContext(Dictionary<string, object> contextEntries)
    {
        foreach (var entry in contextEntries)
        {
            Context[entry.Key] = entry.Value;
        }
        return this;
    }
}
