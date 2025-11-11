namespace WebTemplate.Core.Interfaces
{
    using WebTemplate.Core.DTOs.Auth;

    /// <summary>
    /// Authentication service interface
    /// Handles user authentication, registration, and token management
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticate user with email and password
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>Authentication response with tokens and user info</returns>
        Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <param name="registerDto">Registration information</param>
        /// <returns>Authentication response with tokens and user info</returns>
        Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// Logout user and invalidate tokens
        /// </summary>
        /// <param name="logoutDto">Logout information including refresh token</param>
        /// <returns>Success status</returns>
        Task<ApiResponseDto<bool>> LogoutAsync(LogoutDto logoutDto);

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="refreshTokenDto">Refresh token request</param>
        /// <returns>New authentication response</returns>
        Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);

        /// <summary>
        /// Confirm user email address
        /// </summary>
        /// <param name="confirmEmailDto">Email confirmation data</param>
        /// <returns>Success status</returns>
        Task<ApiResponseDto<bool>> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto);

        /// <summary>
        /// Initiate password reset process
        /// </summary>
        /// <param name="forgotPasswordDto">Password reset request</param>
        /// <returns>Success status</returns>
        Task<ApiResponseDto<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);

        /// <summary>
        /// Reset password using token
        /// </summary>
        /// <param name="resetPasswordDto">Password reset data</param>
        /// <returns>Success status</returns>
        Task<ApiResponseDto<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="changePasswordDto">Password change data</param>
        /// <returns>Success status</returns>
        Task<ApiResponseDto<bool>> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);

        /// <summary>
        /// Get current authentication status
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>Authentication status</returns>
        Task<ApiResponseDto<AuthStatusDto>> GetAuthStatusAsync(string userId);

        /// <summary>
        /// Validate and parse JWT token
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>Token validation result with user claims</returns>
        Task<ApiResponseDto<Dictionary<string, object>>> ValidateTokenAsync(string token);

        /// <summary>
        /// Revoke all tokens for a user (logout from all devices)
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>Success status</returns>
        Task<ApiResponseDto<bool>> RevokeAllTokensAsync(string userId);
    }
}