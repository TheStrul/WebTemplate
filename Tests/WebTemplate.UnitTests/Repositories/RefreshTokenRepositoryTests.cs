namespace WebTemplate.UnitTests.Repositories
{
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using WebTemplate.Core.Entities;
    using WebTemplate.Data.Context;
    using WebTemplate.Data.Repositories;
    using Xunit;

    /// <summary>
    /// Unit tests for RefreshTokenRepository
    /// Tests CRUD operations and query methods using in-memory database
    /// </summary>
    public class RefreshTokenRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly RefreshTokenRepository _repository;
        private readonly string _testUserId = Guid.NewGuid().ToString();
        private readonly string _testUserId2 = Guid.NewGuid().ToString();

        public RefreshTokenRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new RefreshTokenRepository(_context);

            // Seed test users
            SeedTestUsers();
        }

        private void SeedTestUsers()
        {
            var users = new[]
            {
                new ApplicationUser
                {
                    Id = _testUserId,
                    UserName = "testuser1@example.com",
                    Email = "testuser1@example.com",
                    EmailConfirmed = true,
                    UserTypeId = 1
                },
                new ApplicationUser
                {
                    Id = _testUserId2,
                    UserName = "testuser2@example.com",
                    Email = "testuser2@example.com",
                    EmailConfirmed = true,
                    UserTypeId = 1
                }
            };

            _context.Users.AddRange(users);
            _context.SaveChanges();
        }

        #region AddAsync Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task AddAsync_WithValidToken_AddsToDatabase()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "test-token-123",
                UserId = _testUserId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _repository.AddAsync(token);

            // Assert
            var saved = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token.Token);
            saved.Should().NotBeNull();
            saved!.UserId.Should().Be(_testUserId);
            saved.ExpiryDate.Should().BeCloseTo(token.ExpiryDate, TimeSpan.FromSeconds(1));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task AddAsync_WithMultipleTokens_AddsAllToDatabase()
        {
            // Arrange
            var token1 = new RefreshToken
            {
                Token = "token-1",
                UserId = _testUserId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            var token2 = new RefreshToken
            {
                Token = "token-2",
                UserId = _testUserId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _repository.AddAsync(token1);
            await _repository.AddAsync(token2);

            // Assert
            var count = await _context.RefreshTokens.CountAsync();
            count.Should().Be(2);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task UpdateAsync_WithExistingToken_UpdatesDatabase()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "update-test-token",
                UserId = _testUserId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(token);

            // Act
            token.RevokedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(token);

            // Assert
            var updated = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token.Token);
            updated.Should().NotBeNull();
            updated!.RevokedAt.Should().NotBeNull();
        }

        #endregion

        #region UpdateRangeAsync Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task UpdateRangeAsync_WithMultipleTokens_UpdatesAll()
        {
            // Arrange
            var tokens = new[]
            {
                new RefreshToken
                {
                    Token = "range-token-1",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                },
                new RefreshToken
                {
                    Token = "range-token-2",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var token in tokens)
            {
                await _repository.AddAsync(token);
            }

            // Act
            var revokeTime = DateTime.UtcNow;
            foreach (var token in tokens)
            {
                token.RevokedAt = revokeTime;
            }
            await _repository.UpdateRangeAsync(tokens);

            // Assert
            var updated = await _context.RefreshTokens
                .Where(t => t.Token.StartsWith("range-token"))
                .ToListAsync();
            updated.Should().HaveCount(2);
            updated.Should().OnlyContain(t => t.RevokedAt != null);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task DeleteAsync_WithExistingToken_RemovesFromDatabase()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "delete-test-token",
                UserId = _testUserId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(token);

            // Act
            await _repository.DeleteAsync(token);

            // Assert
            var deleted = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token.Token);
            deleted.Should().BeNull();
        }

        #endregion

        #region DeleteRangeAsync Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task DeleteRangeAsync_WithMultipleTokens_RemovesAll()
        {
            // Arrange
            var tokens = new[]
            {
                new RefreshToken
                {
                    Token = "delete-range-1",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                },
                new RefreshToken
                {
                    Token = "delete-range-2",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var token in tokens)
            {
                await _repository.AddAsync(token);
            }

            // Act
            await _repository.DeleteRangeAsync(tokens);

            // Assert
            var remaining = await _context.RefreshTokens
                .Where(t => t.Token.StartsWith("delete-range"))
                .ToListAsync();
            remaining.Should().BeEmpty();
        }

        #endregion

        #region GetByTokenAsync Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetByTokenAsync_WithValidToken_ReturnsToken()
        {
            // Arrange
            var tokenString = "get-token-test";
            var token = new RefreshToken
            {
                Token = tokenString,
                UserId = _testUserId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(token);

            // Act
            var result = await _repository.GetByTokenAsync(tokenString);

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().Be(tokenString);
            result.UserId.Should().Be(_testUserId);
            result.User.Should().NotBeNull(); // Includes user
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetByTokenAsync_WithNonExistentToken_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByTokenAsync("non-existent-token");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetTokensByUserIdAsync Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetTokensByUserIdAsync_WithMultipleTokens_ReturnsAllForUser()
        {
            // Arrange
            var tokens = new[]
            {
                new RefreshToken
                {
                    Token = "user-token-1",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow.AddMinutes(-10)
                },
                new RefreshToken
                {
                    Token = "user-token-2",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow.AddMinutes(-5)
                },
                new RefreshToken
                {
                    Token = "other-user-token",
                    UserId = _testUserId2,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var token in tokens)
            {
                await _repository.AddAsync(token);
            }

            // Act
            var result = await _repository.GetTokensByUserIdAsync(_testUserId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(t => t.UserId == _testUserId);
            result.Should().BeInAscendingOrder(t => t.CreatedAt); // Ordered by CreatedAt
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetTokensByUserIdAsync_WithNoTokens_ReturnsEmpty()
        {
            // Act
            var result = await _repository.GetTokensByUserIdAsync(Guid.NewGuid().ToString());

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetActiveTokensByUserIdAsync Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetActiveTokensByUserIdAsync_ReturnsOnlyActiveTokens()
        {
            // Arrange
            var tokens = new[]
            {
                new RefreshToken // Active
                {
                    Token = "active-1",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    RevokedAt = null
                },
                new RefreshToken // Revoked
                {
                    Token = "revoked-1",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    RevokedAt = DateTime.UtcNow
                },
                new RefreshToken // Expired
                {
                    Token = "expired-1",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    RevokedAt = null
                },
                new RefreshToken // Active
                {
                    Token = "active-2",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow.AddMinutes(1),
                    RevokedAt = null
                }
            };

            foreach (var token in tokens)
            {
                await _repository.AddAsync(token);
            }

            // Act
            var result = await _repository.GetActiveTokensByUserIdAsync(_testUserId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(t => t.Token == "active-1");
            result.Should().Contain(t => t.Token == "active-2");
            result.Should().NotContain(t => t.Token == "revoked-1");
            result.Should().NotContain(t => t.Token == "expired-1");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetActiveTokensByUserIdAsync_WithNoActiveTokens_ReturnsEmpty()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "all-revoked",
                UserId = _testUserId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(token);

            // Act
            var result = await _repository.GetActiveTokensByUserIdAsync(_testUserId);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetExpiredTokensAsync Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetExpiredTokensAsync_ReturnsOnlyExpiredTokens()
        {
            // Arrange
            var tokens = new[]
            {
                new RefreshToken
                {
                    Token = "expired-token-1",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new RefreshToken
                {
                    Token = "expired-token-2",
                    UserId = _testUserId2,
                    ExpiryDate = DateTime.UtcNow.AddHours(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new RefreshToken
                {
                    Token = "active-token",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var token in tokens)
            {
                await _repository.AddAsync(token);
            }

            // Act
            var result = await _repository.GetExpiredTokensAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(t => t.Token == "expired-token-1");
            result.Should().Contain(t => t.Token == "expired-token-2");
            result.Should().NotContain(t => t.Token == "active-token");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetExpiredTokensAsync_WithNoExpiredTokens_ReturnsEmpty()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "future-token",
                UserId = _testUserId,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(token);

            // Act
            var result = await _repository.GetExpiredTokensAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetTokensToCleanupAsync Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetTokensToCleanupAsync_ReturnsExpiredAndRevokedTokens()
        {
            // Arrange
            var tokens = new[]
            {
                new RefreshToken // Expired
                {
                    Token = "cleanup-expired",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    RevokedAt = null
                },
                new RefreshToken // Revoked
                {
                    Token = "cleanup-revoked",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    RevokedAt = DateTime.UtcNow
                },
                new RefreshToken // Expired AND Revoked
                {
                    Token = "cleanup-both",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    RevokedAt = DateTime.UtcNow.AddDays(-1)
                },
                new RefreshToken // Active
                {
                    Token = "cleanup-active",
                    UserId = _testUserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    RevokedAt = null
                }
            };

            foreach (var token in tokens)
            {
                await _repository.AddAsync(token);
            }

            // Act
            var result = await _repository.GetTokensToCleanupAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(t => t.Token == "cleanup-expired");
            result.Should().Contain(t => t.Token == "cleanup-revoked");
            result.Should().Contain(t => t.Token == "cleanup-both");
            result.Should().NotContain(t => t.Token == "cleanup-active");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetTokensToCleanupAsync_WithNoCleanupNeeded_ReturnsEmpty()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "perfect-token",
                UserId = _testUserId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            };
            await _repository.AddAsync(token);

            // Act
            var result = await _repository.GetTokensToCleanupAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
