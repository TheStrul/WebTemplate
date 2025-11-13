namespace WebTemplate.Core.Common;

/// <summary>
/// Centralized repository of predefined errors to avoid magic strings.
/// Follow the naming convention: Category_SpecificError
/// This improves consistency and makes errors easily queryable in Application Insights.
/// </summary>
public static class Errors
{
    #region Authentication Errors

    public static class Auth
    {
        public static readonly Error InvalidCredentials = new(
            "AUTH.INVALID_CREDENTIALS",
            ErrorType.Unauthorized,
            "Invalid email or password."
        );

        public static readonly Error UserNotFound = new(
            "AUTH.USER_NOT_FOUND",
            ErrorType.NotFound,
            "User account not found."
        );

        public static readonly Error UserInactive = new(
            "AUTH.USER_INACTIVE",
            ErrorType.Forbidden,
            "User account is inactive."
        );

        public static readonly Error UserDeleted = new(
            "AUTH.USER_DELETED",
            ErrorType.Forbidden,
            "User account has been deleted."
        );

        public static readonly Error EmailNotConfirmed = new(
            "AUTH.EMAIL_NOT_CONFIRMED",
            ErrorType.Forbidden,
            "Email address has not been confirmed."
        );

        public static readonly Error AccountLocked = new(
            "AUTH.ACCOUNT_LOCKED",
            ErrorType.Forbidden,
            "Account is locked due to multiple failed login attempts."
        );

        public static readonly Error InvalidToken = new(
            "AUTH.INVALID_TOKEN",
            ErrorType.Unauthorized,
            "The provided token is invalid or has expired."
        );

        public static readonly Error InvalidRefreshToken = new(
            "AUTH.INVALID_REFRESH_TOKEN",
            ErrorType.Unauthorized,
            "The provided refresh token is invalid or has expired."
        );

        public static readonly Error TokenExpired = new(
            "AUTH.TOKEN_EXPIRED",
            ErrorType.Unauthorized,
            "The authentication token has expired."
        );

        public static readonly Error PasswordResetFailed = new(
            "AUTH.PASSWORD_RESET_FAILED",
            ErrorType.Failure,
            "Password reset failed."
        );

        public static readonly Error InvalidResetToken = new(
            "AUTH.INVALID_RESET_TOKEN",
            ErrorType.Validation,
            "Invalid or expired password reset token."
        );

        public static readonly Error PasswordChangeFailed = new(
            "AUTH.PASSWORD_CHANGE_FAILED",
            ErrorType.Failure,
            "Password change failed."
        );

        public static readonly Error CurrentPasswordIncorrect = new(
            "AUTH.CURRENT_PASSWORD_INCORRECT",
            ErrorType.Validation,
            "Current password is incorrect."
        );

        public static Error EmailAlreadyExists(string email) => new(
            "AUTH.EMAIL_ALREADY_EXISTS",
            ErrorType.Conflict,
            $"An account with email '{email}' already exists."
        );

        public static Error RegistrationFailed(string reason) => new(
            "AUTH.REGISTRATION_FAILED",
            ErrorType.Failure,
            $"User registration failed: {reason}"
        );
    }

    #endregion

    #region Validation Errors

    public static class Validation
    {
        public static readonly Error InvalidEmail = new(
            "VALIDATION.INVALID_EMAIL",
            ErrorType.Validation,
            "Email address is not in a valid format."
        );

        public static readonly Error InvalidPhoneNumber = new(
            "VALIDATION.INVALID_PHONE",
            ErrorType.Validation,
            "Phone number is not in a valid format."
        );

        public static readonly Error PasswordTooWeak = new(
            "VALIDATION.PASSWORD_TOO_WEAK",
            ErrorType.Validation,
            "Password does not meet security requirements."
        );

        public static readonly Error PasswordMismatch = new(
            "VALIDATION.PASSWORD_MISMATCH",
            ErrorType.Validation,
            "Password and confirmation password do not match."
        );

        public static Error RequiredField(string fieldName) => new(
            "VALIDATION.REQUIRED_FIELD",
            ErrorType.Validation,
            $"The field '{fieldName}' is required."
        );

        public static Error InvalidLength(string fieldName, int min, int max) => new(
            "VALIDATION.INVALID_LENGTH",
            ErrorType.Validation,
            $"The field '{fieldName}' must be between {min} and {max} characters."
        );

        public static Error InvalidFormat(string fieldName, string expectedFormat) => new(
            "VALIDATION.INVALID_FORMAT",
            ErrorType.Validation,
            $"The field '{fieldName}' must be in format: {expectedFormat}"
        );
    }

    #endregion

    #region User Errors

    public static class User
    {
        public static readonly Error NotFound = new(
            "USER.NOT_FOUND",
            ErrorType.NotFound,
            "User not found."
        );

        public static readonly Error CreationFailed = new(
            "USER.CREATION_FAILED",
            ErrorType.Failure,
            "User creation failed."
        );

        public static readonly Error UpdateFailed = new(
            "USER.UPDATE_FAILED",
            ErrorType.Failure,
            "User update failed."
        );

        public static readonly Error DeletionFailed = new(
            "USER.DELETION_FAILED",
            ErrorType.Failure,
            "User deletion failed."
        );

        public static Error NotFoundById(string userId) => new(
            "USER.NOT_FOUND_BY_ID",
            ErrorType.NotFound,
            $"User with ID '{userId}' not found."
        );

        public static Error NotFoundByEmail(string email) => new(
            "USER.NOT_FOUND_BY_EMAIL",
            ErrorType.NotFound,
            $"User with email '{email}' not found."
        );
    }

    #endregion

    #region UserType Errors

    public static class UserType
    {
        public static readonly Error NotFound = new(
            "USERTYPE.NOT_FOUND",
            ErrorType.NotFound,
            "User type not found."
        );

        public static readonly Error CreationFailed = new(
            "USERTYPE.CREATION_FAILED",
            ErrorType.Failure,
            "User type creation failed."
        );

        public static readonly Error UpdateFailed = new(
            "USERTYPE.UPDATE_FAILED",
            ErrorType.Failure,
            "User type update failed."
        );

        public static readonly Error DeletionFailed = new(
            "USERTYPE.DELETION_FAILED",
            ErrorType.Failure,
            "User type deletion failed."
        );

        public static readonly Error InUse = new(
            "USERTYPE.IN_USE",
            ErrorType.Conflict,
            "Cannot delete user type because it is assigned to existing users."
        );

        public static Error AlreadyExists(string name) => new(
            "USERTYPE.ALREADY_EXISTS",
            ErrorType.Conflict,
            $"User type with name '{name}' already exists."
        );

        public static Error NotFoundById(int id) => new(
            "USERTYPE.NOT_FOUND_BY_ID",
            ErrorType.NotFound,
            $"User type with ID '{id}' not found."
        );
    }

    #endregion

    #region Database Errors

    public static class Database
    {
        public static readonly Error ConnectionFailed = new(
            "DATABASE.CONNECTION_FAILED",
            ErrorType.Database,
            "Failed to connect to the database."
        );

        public static readonly Error QueryFailed = new(
            "DATABASE.QUERY_FAILED",
            ErrorType.Database,
            "Database query failed."
        );

        public static readonly Error TransactionFailed = new(
            "DATABASE.TRANSACTION_FAILED",
            ErrorType.Database,
            "Database transaction failed."
        );

        public static readonly Error ConcurrencyConflict = new(
            "DATABASE.CONCURRENCY_CONFLICT",
            ErrorType.Conflict,
            "The record has been modified by another user. Please refresh and try again."
        );

        public static Error SaveFailed(string entityType) => new(
            "DATABASE.SAVE_FAILED",
            ErrorType.Database,
            $"Failed to save {entityType} to the database."
        );
    }

    #endregion

    #region Email Errors

    public static class Email
    {
        public static readonly Error SendFailed = new(
            "EMAIL.SEND_FAILED",
            ErrorType.External,
            "Failed to send email."
        );

        public static readonly Error InvalidConfiguration = new(
            "EMAIL.INVALID_CONFIGURATION",
            ErrorType.Configuration,
            "Email service is not properly configured."
        );

        public static Error SendFailedToRecipient(string recipient) => new(
            "EMAIL.SEND_FAILED_TO_RECIPIENT",
            ErrorType.External,
            $"Failed to send email to '{recipient}'."
        );
    }

    #endregion

    #region Configuration Errors

    public static class Configuration
    {
        public static readonly Error SectionMissing = new(
            "CONFIG.SECTION_MISSING",
            ErrorType.Configuration,
            "Required configuration section is missing."
        );

        public static readonly Error InvalidValue = new(
            "CONFIG.INVALID_VALUE",
            ErrorType.Configuration,
            "Configuration value is invalid."
        );

        public static readonly Error RequiredValueMissing = new(
            "CONFIG.REQUIRED_VALUE_MISSING",
            ErrorType.Configuration,
            "Required configuration value is missing."
        );

        public static Error SectionMissingOrInvalid(string sectionName) => new(
            "CONFIG.SECTION_MISSING_OR_INVALID",
            ErrorType.Configuration,
            $"Required configuration section '{sectionName}' is missing or invalid."
        );

        public static Error RequiredFieldMissing(string fieldPath) => new(
            "CONFIG.REQUIRED_FIELD_MISSING",
            ErrorType.Configuration,
            $"{fieldPath} is required but not configured."
        );

        public static Error RequiredFieldMissingWithGuidance(string fieldPath, string guidance) => new(
            "CONFIG.REQUIRED_FIELD_MISSING",
            ErrorType.Configuration,
            $"{fieldPath} is required but not configured. {guidance}"
        );

        public static Error InvalidFormat(string fieldPath, string reason) => new(
            "CONFIG.INVALID_FORMAT",
            ErrorType.Configuration,
            $"{fieldPath} is not valid: {reason}"
        );

        public static Error ValueOutOfRange(string fieldPath, string constraint) => new(
            "CONFIG.VALUE_OUT_OF_RANGE",
            ErrorType.Configuration,
            $"{fieldPath} {constraint}"
        );

        public static Error InvalidUrl(string fieldPath, string url) => new(
            "CONFIG.INVALID_URL",
            ErrorType.Configuration,
            $"{fieldPath} '{url}' is not a valid absolute URL."
        );
    }

    #endregion

    #region General Errors

    public static class General
    {
        public static readonly Error UnexpectedError = new(
            "GENERAL.UNEXPECTED_ERROR",
            ErrorType.Failure,
            "An unexpected error occurred."
        );

        public static readonly Error NotImplemented = new(
            "GENERAL.NOT_IMPLEMENTED",
            ErrorType.Failure,
            "This feature is not yet implemented."
        );

        public static readonly Error Forbidden = new(
            "GENERAL.FORBIDDEN",
            ErrorType.Forbidden,
            "You do not have permission to perform this action."
        );

        public static readonly Error Timeout = new(
            "GENERAL.TIMEOUT",
            ErrorType.Timeout,
            "The operation timed out."
        );

        public static readonly Error RateLimitExceeded = new(
            "GENERAL.RATE_LIMIT_EXCEEDED",
            ErrorType.RateLimit,
            "Too many requests. Please try again later."
        );

        public static Error InvalidOperation(string operation) => new(
            "GENERAL.INVALID_OPERATION",
            ErrorType.BusinessRule,
            $"Invalid operation: {operation}"
        );

        public static Error ServiceUnavailable(string serviceName) => new(
            "GENERAL.SERVICE_UNAVAILABLE",
            ErrorType.External,
            $"Service '{serviceName}' is currently unavailable."
        );
    }

    #endregion
}
