namespace WebTemplate.Core.DTOs.Auth
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Login request DTO - All validation messages configurable
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;

        /// <summary>
        /// Optional device/client identifier for session management
        /// </summary>
        [StringLength(100)]
        public string? DeviceId { get; set; }

        /// <summary>
        /// Optional device name for user-friendly session identification
        /// </summary>
        [StringLength(100)]
        public string? DeviceName { get; set; }
    }

    /// <summary>
    /// User registration request DTO
    /// </summary>
    public class RegisterDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// User type ID (default to regular user type)
        /// </summary>
        public int UserTypeId { get; set; } = 2;

        /// <summary>
        /// Terms and conditions acceptance
        /// </summary>
        [Required(ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTerms { get; set; }

        /// <summary>
        /// Privacy policy acceptance
        /// </summary>
        [Required(ErrorMessage = "You must accept the privacy policy")]
        public bool AcceptPrivacyPolicy { get; set; }

        /// <summary>
        /// Newsletter subscription (optional)
        /// </summary>
        public bool SubscribeToNewsletter { get; set; } = false;
    }

    /// <summary>
    /// Authentication response DTO containing tokens and user info
    /// </summary>
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiration { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public UserProfileDto User { get; set; } = null!;
        public List<string> Permissions { get; set; } = new();
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// Refresh token request DTO
    /// </summary>
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Access token is required")]
        public string AccessToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Password reset request DTO
    /// </summary>
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Password reset confirmation DTO
    /// </summary>
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reset token is required")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Change password request DTO
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Email confirmation DTO
    /// </summary>
    public class ConfirmEmailDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmation token is required")]
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// Logout request DTO
    /// </summary>
    public class LogoutDto
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Logout from all devices
        /// </summary>
        public bool LogoutFromAllDevices { get; set; } = false;
    }

    /// <summary>
    /// Two-factor authentication setup DTO
    /// </summary>
    public class TwoFactorSetupDto
    {
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits")]
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Two-factor authentication verification DTO
    /// </summary>
    public class TwoFactorVerificationDto
    {
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits")]
        public string Code { get; set; } = string.Empty;

        public bool RememberDevice { get; set; } = false;
    }

    /// <summary>
    /// Generic API response wrapper
    /// </summary>
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, object> Meta { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Authentication status DTO
    /// </summary>
    public class AuthStatusDto
    {
        public bool IsAuthenticated { get; set; }
        public UserProfileDto? User { get; set; }
        public List<string> Permissions { get; set; } = new();
        public DateTime? TokenExpiration { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}