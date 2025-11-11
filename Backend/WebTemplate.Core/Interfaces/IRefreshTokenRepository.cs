using WebTemplate.Core.Entities;

namespace WebTemplate.Core.Interfaces
{
    /// <summary>
    /// Refresh token repository interface
    /// Handles CRUD operations for refresh tokens
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Add new refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token entity</param>
        /// <returns>Task</returns>
        Task AddAsync(RefreshToken refreshToken);

        /// <summary>
        /// Update refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token entity</param>
        /// <returns>Task</returns>
        Task UpdateAsync(RefreshToken refreshToken);

        /// <summary>
        /// Update multiple refresh tokens
        /// </summary>
        /// <param name="refreshTokens">Refresh token entities</param>
        /// <returns>Task</returns>
        Task UpdateRangeAsync(IEnumerable<RefreshToken> refreshTokens);

        /// <summary>
        /// Delete refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token entity</param>
        /// <returns>Task</returns>
        Task DeleteAsync(RefreshToken refreshToken);

        /// <summary>
        /// Delete multiple refresh tokens
        /// </summary>
        /// <param name="refreshTokens">Refresh token entities</param>
        /// <returns>Task</returns>
        Task DeleteRangeAsync(IEnumerable<RefreshToken> refreshTokens);

        /// <summary>
        /// Get refresh token by token string
        /// </summary>
        /// <param name="token">Token string</param>
        /// <returns>Refresh token or null</returns>
        Task<RefreshToken?> GetByTokenAsync(string token);

        /// <summary>
        /// Get all tokens for a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>List of refresh tokens</returns>
        Task<IEnumerable<RefreshToken>> GetTokensByUserIdAsync(string userId);

        /// <summary>
        /// Get active tokens for a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>List of active refresh tokens</returns>
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);

        /// <summary>
        /// Get expired tokens for cleanup
        /// </summary>
        /// <returns>List of expired refresh tokens</returns>
        Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync();

        /// <summary>
        /// Get tokens that should be cleaned up (expired or revoked)
        /// </summary>
        /// <returns>List of tokens to clean up</returns>
        Task<IEnumerable<RefreshToken>> GetTokensToCleanupAsync();
    }
}