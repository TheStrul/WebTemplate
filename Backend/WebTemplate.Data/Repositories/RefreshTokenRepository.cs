using Microsoft.EntityFrameworkCore;
using WebTemplate.Core.Entities;
using WebTemplate.Core.Interfaces;
using WebTemplate.Data.Context;

namespace WebTemplate.Data.Repositories
{
    /// <summary>
    /// Refresh token repository implementation
    /// Handles CRUD operations for refresh tokens using Entity Framework
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<RefreshToken> refreshTokens)
        {
            _context.RefreshTokens.UpdateRange(refreshTokens);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<RefreshToken> refreshTokens)
        {
            _context.RefreshTokens.RemoveRange(refreshTokens);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<IEnumerable<RefreshToken>> GetTokensByUserIdAsync(string userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .OrderBy(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && 
                           rt.RevokedAt == null && 
                           rt.ExpiryDate > DateTime.UtcNow)
                .OrderBy(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync()
        {
            return await _context.RefreshTokens
                .Where(rt => rt.ExpiryDate <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetTokensToCleanupAsync()
        {
            return await _context.RefreshTokens
                .Where(rt => rt.ExpiryDate <= DateTime.UtcNow || rt.RevokedAt != null)
                .ToListAsync();
        }
    }
}