# Result Pattern Implementation

## Overview

This project implements the **Result Pattern** for structured error handling that works seamlessly with Azure Application Insights. The pattern provides a clean, explicit way to handle both success and failure cases without relying on exceptions for control flow.

## Key Components

### 1. `Result` and `Result<T>`

Located in: `WebTemplate.Core/Common/Result.cs`

- `Result` - For operations that don't return data
- `Result<T>` - For operations that return data on success

### 2. `Error` Record

Located in: `WebTemplate.Core/Common/Error.cs`

Structured error representation with:
- **Code**: Unique identifier (e.g., "AUTH.INVALID_TOKEN")
- **Type**: Error category (Validation, NotFound, Unauthorized, etc.)
- **Description**: Human-readable message
- **Exception**: Optional inner exception
- **Metadata**: Dictionary for structured logging to Application Insights

### 3. `Errors` Static Class

Located in: `WebTemplate.Core/Common/Errors.cs`

Centralized repository of predefined errors organized by category:
- Auth
- Validation
- User
- UserType
- Database
- Email
- General

## Usage Examples

### Basic Success/Failure

```csharp
// Simple success
public Result DoSomething()
{
    // ... operation logic
    return Result.Success();
}

// Success with data
public Result<User> GetUser(string userId)
{
    var user = await _userRepository.GetByIdAsync(userId);

    if (user == null)
        return Errors.User.NotFoundById(userId);

    return user; // Implicit conversion to Result<User>
}

// Failure with predefined error
public Result<User> ValidateUser(User user)
{
    if (string.IsNullOrEmpty(user.Email))
        return Errors.Validation.InvalidEmail;

    return Result<User>.Success(user);
}
```

### With Context Metadata (for Application Insights)

```csharp
public async Task<Result<User>> CreateUserAsync(CreateUserDto dto)
{
    try
    {
        var user = new User { Email = dto.Email };
        await _userRepository.AddAsync(user);

        return Result<User>.Success(user)
            .WithContext("userId", user.Id)
            .WithContext("email", user.Email)
            .WithContext("createdAt", DateTime.UtcNow);
    }
    catch (Exception ex)
    {
        return Error.FromException(ex, "USER.CREATION_ERROR")
            .WithMetadata("email", dto.Email)
            .WithMetadata("attemptedAt", DateTime.UtcNow);
    }
}
```

### Multiple Errors

```csharp
public Result<User> ValidateUserRegistration(RegisterDto dto)
{
    var errors = new List<Error>();

    if (string.IsNullOrEmpty(dto.Email))
        errors.Add(Errors.Validation.RequiredField("Email"));

    if (dto.Password.Length < 8)
        errors.Add(Errors.Validation.PasswordTooWeak);

    if (dto.Password != dto.ConfirmPassword)
        errors.Add(Errors.Validation.PasswordMismatch);

    if (errors.Any())
        return Result<User>.Failure(errors);

    return Result<User>.Success(new User());
}
```

### Handling Results

```csharp
public async Task<IActionResult> GetUserById(string userId)
{
    var result = await _userService.GetUserAsync(userId);

    if (result.IsFailure)
    {
        // Log to Application Insights with structured data
        _logger.LogError(
            "User retrieval failed: {ErrorCode} - {ErrorDescription}",
            result.Error.Code,
            result.Error.Description
        );

        // Map error type to HTTP status
        return result.Error.Type switch
        {
            ErrorType.NotFound => NotFound(result.Error.Description),
            ErrorType.Validation => BadRequest(result.Error.Description),
            ErrorType.Unauthorized => Unauthorized(result.Error.Description),
            _ => StatusCode(500, result.Error.Description)
        };
    }

    return Ok(result.Value);
}
```

### Converting Result to ApiResponseDto (for API responses)

```csharp
public static class ResultExtensions
{
    public static ApiResponseDto<T> ToApiResponse<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new ApiResponseDto<T>
            {
                Success = true,
                Data = result.Value,
                Meta = result.Context,
                Timestamp = DateTime.UtcNow
            };
        }

        return new ApiResponseDto<T>
        {
            Success = false,
            Message = result.Error?.Description ?? "An error occurred",
            Errors = result.Errors.Select(e => e.Description).ToList(),
            Meta = result.Error?.Metadata ?? new Dictionary<string, object>(),
            Timestamp = DateTime.UtcNow
        };
    }
}
```

## Best Practices

### ✅ DO

- Use `Result` for business logic layer (services)
- Use predefined errors from `Errors` class
- Add context metadata for debugging and telemetry
- Return `Result` for operations that can fail
- Use implicit conversions for cleaner code
- Log failures with structured data

### ❌ DON'T

- Use Result in repository/data layer (let exceptions bubble up)
- Return Result from API controllers (use `ApiResponseDto` instead)
- Create ad-hoc error strings (use predefined `Errors`)
- Use Result for simple, always-successful operations
- Ignore the context metadata - it's valuable for debugging

## Azure Application Insights Integration

The Result Pattern is designed to work seamlessly with Application Insights:

### Structured Logging

```csharp
if (result.IsFailure)
{
    _logger.LogError(
        "Operation failed: {ErrorCode} - {ErrorType} | {ErrorDescription} | Context: {@Context}",
        result.Error.Code,
        result.Error.Type,
        result.Error.Description,
        result.Context
    );
}
```

This creates queryable custom dimensions in Application Insights:
- `customDimensions.prop__ErrorCode`
- `customDimensions.prop__ErrorType`
- `customDimensions.prop__Context`

### Querying in Application Insights (KQL)

```kql
// Find all authentication errors
traces
| where customDimensions.prop__ErrorCode startswith "AUTH."
| project timestamp, message, errorCode = customDimensions.prop__ErrorCode

// Find specific error types
traces
| where customDimensions.prop__ErrorType == "Validation"
| summarize count() by tostring(customDimensions.prop__ErrorCode)

// Track user operations
traces
| where customDimensions.prop__userId != ""
| project timestamp, operation_Name, userId = customDimensions.prop__userId
```

## Error Type Mapping to HTTP Status Codes

| ErrorType | HTTP Status | Description |
|-----------|-------------|-------------|
| Validation | 400 | Bad Request |
| NotFound | 404 | Not Found |
| Unauthorized | 401 | Unauthorized |
| Forbidden | 403 | Forbidden |
| Conflict | 409 | Conflict |
| BusinessRule | 422 | Unprocessable Entity |
| Failure | 500 | Internal Server Error |
| External | 502 | Bad Gateway |
| Database | 500 | Internal Server Error |
| Timeout | 504 | Gateway Timeout |
| RateLimit | 429 | Too Many Requests |

## Adding New Errors

When adding new errors, follow this structure in `Errors.cs`:

```csharp
public static class YourCategory
{
    // Static predefined errors
    public static readonly Error CommonError = new(
        "CATEGORY.ERROR_CODE",
        ErrorType.Validation,
        "Human-readable description"
    );

    // Dynamic errors with parameters
    public static Error ParameterizedError(string param) => new(
        "CATEGORY.PARAMETERIZED",
        ErrorType.NotFound,
        $"Resource '{param}' not found."
    );
}
```

## Migration from ApiResponseDto

**ApiResponseDto** remains for API responses at the controller level.

**Result Pattern** is for internal service/business logic layer.

```
Controller (API Layer)
    ↓ calls
Service (Business Logic) → Returns Result<T>
    ↓ converts to
Controller → Returns ApiResponseDto<T>
```

## References

- [The Result Pattern in C# (Medium)](https://medium.com/@aseem2372005/the-result-pattern-in-c-a-smarter-way-to-handle-errors-c6dee28a0ef0)
- [Azure Application Insights Structured Logging](https://www.bounteous.com/insights/2021/05/04/structured-logging-microsofts-azure-application-insights/)
- [Comprehensive Guide to Azure Telemetry](https://medium.com/@almaskhanwazir/comprehensive-guide-to-azure-telemetry-for-logging-errors-and-more-in-c-c279cdc97f57)
