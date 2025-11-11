namespace WebTemplate.Core.Interfaces
{
    using System.Security.Claims;

    /// <summary>
    /// JWT token service interface
    /// Handles JWT token generation, validation, and management
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generate access token for authenticated user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="email">User email</param>
        /// <param name="roles">User roles</param>
        /// <param name="additionalClaims">Additional claims to include</param>
        /// <returns>JWT access token</returns>
        Task<string> GenerateAccessTokenAsync(string userId, string email, IEnumerable<string> roles, Dictionary<string, object>? additionalClaims = null);

        /// <summary>
        /// Generate refresh token for user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="deviceId">Optional device identifier</param>
        /// <returns>Refresh token</returns>
        Task<string> GenerateRefreshTokenAsync(string userId, string? deviceId = null);

        /// <summary>
        /// Validate JWT access token
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>Claims principal if valid, null if invalid</returns>
        Task<ClaimsPrincipal?> ValidateAccessTokenAsync(string token);

        /// <summary>
        /// Validate refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token to validate</param>
        /// <returns>User ID if valid, null if invalid</returns>
        Task<string?> ValidateRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token to revoke</param>
        /// <returns>Success status</returns>
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Revoke all refresh tokens for a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>Success status</returns>
        Task<bool> RevokeAllRefreshTokensAsync(string userId);

        /// <summary>
        /// Get token expiration date
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Expiration date or null if invalid</returns>
        DateTime? GetTokenExpiration(string token);

        /// <summary>
        /// Extract user ID from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID or null if invalid</returns>
        string? GetUserIdFromToken(string token);

        /// <summary>
        /// Clean up expired refresh tokens
        /// </summary>
        /// <returns>Number of tokens cleaned up</returns>
        Task<int> CleanupExpiredTokensAsync();
    }
}