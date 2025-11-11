namespace WebTemplate.UnitTests.Repositories
{
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using WebTemplate.Core.Entities;
    using WebTemplate.Data.Context;
    using WebTemplate.Data.Repositories;
    using Xunit;

    /// <summary>
    /// Unit tests for UserTypeRepository
    /// Tests repository operations using in-memory database
    /// </summary>
    public class UserTypeRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserTypeRepository _repository;

        public UserTypeRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new UserTypeRepository(_context);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var userTypes = new[]
            {
                new UserType
                {
                    Id = 1,
                    Name = "Admin",
                    Description = "Administrator",
                    Permissions = "[\"read\",\"write\",\"delete\",\"admin\"]"
                },
                new UserType
                {
                    Id = 2,
                    Name = "User",
                    Description = "Standard User",
                    Permissions = "[\"read\",\"write\"]"
                },
                new UserType
                {
                    Id = 3,
                    Name = "Moderator",
                    Description = "Moderator",
                    Permissions = "[\"read\",\"write\",\"moderate\"]"
                }
            };

            _context.UserTypes.AddRange(userTypes);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetByIdAsync_WithValidId_ReturnsUserType()
        {
            // Arrange
            var expectedId = 1;

            // Act
            var result = await _repository.GetByIdAsync(expectedId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(expectedId);
            result.Name.Should().Be("Admin");
            result.Description.Should().Be("Administrator");
            result.Permissions.Should().Contain("admin");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetByIdAsync_WithMultipleIds_ReturnsCorrectUserTypes()
        {
            // Act
            var admin = await _repository.GetByIdAsync(1);
            var user = await _repository.GetByIdAsync(2);
            var moderator = await _repository.GetByIdAsync(3);

            // Assert
            admin.Should().NotBeNull();
            admin!.Name.Should().Be("Admin");

            user.Should().NotBeNull();
            user!.Name.Should().Be("User");

            moderator.Should().NotBeNull();
            moderator!.Name.Should().Be("Moderator");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            var nonExistentId = 999;

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetByIdAsync_WithZeroId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(0);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetByIdAsync_WithNegativeId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(-1);

            // Assert
            result.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
