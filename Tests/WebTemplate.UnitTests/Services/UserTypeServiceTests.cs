namespace WebTemplate.UnitTests.Services
{
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using WebTemplate.Core.DTOs;
    using WebTemplate.Core.Entities;
    using WebTemplate.Core.Interfaces;
    using WebTemplate.Core.Services;
    using Xunit;

    public class UserTypeServiceTests
    {
        private readonly Mock<IUserTypeRepository> _mockUserTypeRepository;
        private readonly Mock<ILogger<UserTypeService>> _mockLogger;
        private readonly UserTypeService _userTypeService;

        public UserTypeServiceTests()
        {
            _mockUserTypeRepository = new Mock<IUserTypeRepository>();
            _mockLogger = new Mock<ILogger<UserTypeService>>();

            _userTypeService = new UserTypeService(
                _mockUserTypeRepository.Object,
                _mockLogger.Object);
        }

        #region GetAllUserTypesAsync Tests

        [Fact]
        public async Task GetAllUserTypesAsync_ReturnsAllUserTypes()
        {
            // Arrange
            var userTypes = new List<UserType>
            {
                CreateUserType(1, "Admin", "Administrator access"),
                CreateUserType(2, "User", "Standard user access"),
                CreateUserType(3, "Moderator", "Moderator access")
            };

            _mockUserTypeRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(userTypes);

            // Act
            var result = await _userTypeService.GetAllUserTypesAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(ut => ut.Name == "Admin");
            result.Should().Contain(ut => ut.Name == "User");
            result.Should().Contain(ut => ut.Name == "Moderator");
        }

        [Fact]
        public async Task GetAllUserTypesAsync_WithNoUserTypes_ReturnsEmptyList()
        {
            // Arrange
            _mockUserTypeRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<UserType>());

            // Act
            var result = await _userTypeService.GetAllUserTypesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllUserTypesAsync_WithPermissions_DeserializesPermissions()
        {
            // Arrange
            var userType = CreateUserType(1, "Admin", "Admin type");
            userType.Permissions = "[\"create\",\"read\",\"update\",\"delete\"]";

            _mockUserTypeRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<UserType> { userType });

            // Act
            var result = await _userTypeService.GetAllUserTypesAsync();

            // Assert
            var firstUserType = result.First();
            firstUserType.Permissions.Should().HaveCount(4);
            firstUserType.Permissions.Should().Contain("create");
            firstUserType.Permissions.Should().Contain("read");
            firstUserType.Permissions.Should().Contain("update");
            firstUserType.Permissions.Should().Contain("delete");
        }

        [Fact]
        public async Task GetAllUserTypesAsync_WithInvalidPermissionsJson_ReturnsEmptyPermissions()
        {
            // Arrange
            var userType = CreateUserType(1, "Admin", "Admin type");
            userType.Permissions = "invalid json {[";

            _mockUserTypeRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<UserType> { userType });

            // Act
            var result = await _userTypeService.GetAllUserTypesAsync();

            // Assert
            var firstUserType = result.First();
            firstUserType.Permissions.Should().BeEmpty();
        }

        #endregion

        #region GetUserTypeByIdAsync Tests

        [Fact]
        public async Task GetUserTypeByIdAsync_WithValidId_ReturnsUserType()
        {
            // Arrange
            var userType = CreateUserType(1, "Admin", "Administrator access");
            userType.Permissions = "[\"all\"]";

            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(userType);

            // Act
            var result = await _userTypeService.GetUserTypeByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("Admin");
            result.Description.Should().Be("Administrator access");
            result.Permissions.Should().Contain("all");
        }

        [Fact]
        public async Task GetUserTypeByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserType?)null);

            // Act
            var result = await _userTypeService.GetUserTypeByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateUserTypeAsync Tests

        [Fact]
        public async Task CreateUserTypeAsync_WithValidData_CreatesAndReturnsId()
        {
            // Arrange
            var createDto = new CreateUserTypeDto
            {
                Name = "NewUserType",
                Description = "A new user type",
                Permissions = new List<string> { "read", "write" }
            };

            _mockUserTypeRepository.Setup(r => r.GetByNameAsync("NewUserType"))
                .ReturnsAsync((UserType?)null);
            _mockUserTypeRepository.Setup(r => r.AddAsync(It.IsAny<UserType>()))
                .ReturnsAsync(123);

            // Act
            var result = await _userTypeService.CreateUserTypeAsync(createDto);

            // Assert
            result.Should().Be(123);
            _mockUserTypeRepository.Verify(r => r.AddAsync(It.Is<UserType>(ut =>
                ut.Name == "NewUserType" &&
                ut.Description == "A new user type" &&
                ut.IsActive &&
                ut.Permissions!.Contains("read") &&
                ut.Permissions.Contains("write")
            )), Times.Once);
        }

        [Fact]
        public async Task CreateUserTypeAsync_WithDuplicateName_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingUserType = CreateUserType(1, "ExistingType", "Existing");
            var createDto = new CreateUserTypeDto
            {
                Name = "ExistingType",
                Description = "Try to duplicate",
                Permissions = new List<string>()
            };

            _mockUserTypeRepository.Setup(r => r.GetByNameAsync("ExistingType"))
                .ReturnsAsync(existingUserType);

            // Act
            Func<Task> act = async () => await _userTypeService.CreateUserTypeAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("UserType with name 'ExistingType' already exists");
            _mockUserTypeRepository.Verify(r => r.AddAsync(It.IsAny<UserType>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserTypeAsync_WithCaseInsensitiveDuplicate_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingUserType = CreateUserType(1, "ExistingType", "Existing");
            var createDto = new CreateUserTypeDto
            {
                Name = "existingtype", // Different case
                Description = "Try to duplicate",
                Permissions = new List<string>()
            };

            _mockUserTypeRepository.Setup(r => r.GetByNameAsync("existingtype"))
                .ReturnsAsync(existingUserType);

            // Act
            Func<Task> act = async () => await _userTypeService.CreateUserTypeAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CreateUserTypeAsync_WithEmptyPermissions_CreatesWithEmptyArray()
        {
            // Arrange
            var createDto = new CreateUserTypeDto
            {
                Name = "MinimalType",
                Description = "Minimal",
                Permissions = new List<string>()
            };

            _mockUserTypeRepository.Setup(r => r.GetByNameAsync("MinimalType"))
                .ReturnsAsync((UserType?)null);
            _mockUserTypeRepository.Setup(r => r.AddAsync(It.IsAny<UserType>()))
                .ReturnsAsync(456);

            // Act
            var result = await _userTypeService.CreateUserTypeAsync(createDto);

            // Assert
            result.Should().Be(456);
            _mockUserTypeRepository.Verify(r => r.AddAsync(It.Is<UserType>(ut =>
                ut.Permissions == "[]"
            )), Times.Once);
        }

        #endregion

        #region UpdateUserTypeAsync Tests

        [Fact]
        public async Task UpdateUserTypeAsync_WithValidData_UpdatesAndReturnsTrue()
        {
            // Arrange
            var existingUserType = CreateUserType(1, "OldName", "Old description");
            var updateDto = new CreateUserTypeDto
            {
                Name = "UpdatedName",
                Description = "Updated description",
                Permissions = new List<string> { "permission1", "permission2" }
            };

            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingUserType);
            _mockUserTypeRepository.Setup(r => r.GetByNameAsync("UpdatedName"))
                .ReturnsAsync((UserType?)null);
            _mockUserTypeRepository.Setup(r => r.UpdateAsync(It.IsAny<UserType>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userTypeService.UpdateUserTypeAsync(1, updateDto);

            // Assert
            result.Should().BeTrue();
            existingUserType.Name.Should().Be("UpdatedName");
            existingUserType.Description.Should().Be("Updated description");
            existingUserType.UpdatedAt.Should().NotBeNull();
            _mockUserTypeRepository.Verify(r => r.UpdateAsync(existingUserType), Times.Once);
        }

        [Fact]
        public async Task UpdateUserTypeAsync_WithNonExistentId_ReturnsFalse()
        {
            // Arrange
            var updateDto = new CreateUserTypeDto
            {
                Name = "UpdatedName",
                Description = "Updated",
                Permissions = new List<string>()
            };

            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserType?)null);

            // Act
            var result = await _userTypeService.UpdateUserTypeAsync(999, updateDto);

            // Assert
            result.Should().BeFalse();
            _mockUserTypeRepository.Verify(r => r.UpdateAsync(It.IsAny<UserType>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserTypeAsync_WithDuplicateNameDifferentId_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingUserType1 = CreateUserType(1, "Type1", "First");
            var existingUserType2 = CreateUserType(2, "Type2", "Second");
            var updateDto = new CreateUserTypeDto
            {
                Name = "Type2", // Trying to rename Type1 to Type2
                Description = "Updated",
                Permissions = new List<string>()
            };

            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingUserType1);
            _mockUserTypeRepository.Setup(r => r.GetByNameAsync("Type2"))
                .ReturnsAsync(existingUserType2);

            // Act
            Func<Task> act = async () => await _userTypeService.UpdateUserTypeAsync(1, updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("UserType with name 'Type2' already exists");
        }

        [Fact]
        public async Task UpdateUserTypeAsync_WithSameName_UpdatesSuccessfully()
        {
            // Arrange
            var existingUserType = CreateUserType(1, "SameName", "Old description");
            var updateDto = new CreateUserTypeDto
            {
                Name = "SameName", // Same name, just updating description
                Description = "New description",
                Permissions = new List<string>()
            };

            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingUserType);
            _mockUserTypeRepository.Setup(r => r.GetByNameAsync("SameName"))
                .ReturnsAsync(existingUserType); // Returns same object
            _mockUserTypeRepository.Setup(r => r.UpdateAsync(It.IsAny<UserType>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userTypeService.UpdateUserTypeAsync(1, updateDto);

            // Assert
            result.Should().BeTrue();
            existingUserType.Description.Should().Be("New description");
        }

        #endregion

        #region DeleteUserTypeAsync Tests

        [Fact]
        public async Task DeleteUserTypeAsync_WithNoAssignedUsers_DeletesAndReturnsTrue()
        {
            // Arrange
            var userType = CreateUserType(1, "ToDelete", "Will be deleted");

            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(userType);
            _mockUserTypeRepository.Setup(r => r.CountUsersWithTypeAsync(1))
                .ReturnsAsync(0);
            _mockUserTypeRepository.Setup(r => r.DeleteAsync(userType))
                .ReturnsAsync(true);

            // Act
            var result = await _userTypeService.DeleteUserTypeAsync(1);

            // Assert
            result.Should().BeTrue();
            _mockUserTypeRepository.Verify(r => r.DeleteAsync(userType), Times.Once);
        }

        [Fact]
        public async Task DeleteUserTypeAsync_WithAssignedUsers_ThrowsInvalidOperationException()
        {
            // Arrange
            var userType = CreateUserType(1, "InUse", "Has users");

            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(userType);
            _mockUserTypeRepository.Setup(r => r.CountUsersWithTypeAsync(1))
                .ReturnsAsync(5); // 5 users assigned

            // Act
            Func<Task> act = async () => await _userTypeService.DeleteUserTypeAsync(1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot delete UserType 'InUse' because 5 user(s) are assigned to it");
            _mockUserTypeRepository.Verify(r => r.DeleteAsync(It.IsAny<UserType>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserTypeAsync_WithNonExistentId_ReturnsFalse()
        {
            // Arrange
            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserType?)null);

            // Act
            var result = await _userTypeService.DeleteUserTypeAsync(999);

            // Assert
            result.Should().BeFalse();
            _mockUserTypeRepository.Verify(r => r.DeleteAsync(It.IsAny<UserType>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserTypeAsync_WithMultipleAssignedUsers_IncludesCountInMessage()
        {
            // Arrange
            var userType = CreateUserType(1, "Popular", "Many users");

            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(userType);
            _mockUserTypeRepository.Setup(r => r.CountUsersWithTypeAsync(1))
                .ReturnsAsync(100);

            // Act
            Func<Task> act = async () => await _userTypeService.DeleteUserTypeAsync(1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*100 user(s)*");
        }

        #endregion

        #region ToggleUserTypeStatusAsync Tests

        [Fact]
        public async Task ToggleUserTypeStatusAsync_WithActiveUserType_DeactivatesAndReturnsTrue()
        {
            // Arrange
            var userType = CreateUserType(1, "ActiveType", "Currently active");
            userType.IsActive = true;

            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(userType);
            _mockUserTypeRepository.Setup(r => r.UpdateAsync(It.IsAny<UserType>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userTypeService.ToggleUserTypeStatusAsync(1);

            // Assert
            result.Should().BeTrue();
            userType.IsActive.Should().BeFalse();
            userType.UpdatedAt.Should().NotBeNull();
            _mockUserTypeRepository.Verify(r => r.UpdateAsync(userType), Times.Once);
        }

        [Fact]
        public async Task ToggleUserTypeStatusAsync_WithInactiveUserType_ActivatesAndReturnsTrue()
        {
            // Arrange
            var userType = CreateUserType(1, "InactiveType", "Currently inactive");
            userType.IsActive = false;

            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(userType);
            _mockUserTypeRepository.Setup(r => r.UpdateAsync(It.IsAny<UserType>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userTypeService.ToggleUserTypeStatusAsync(1);

            // Assert
            result.Should().BeTrue();
            userType.IsActive.Should().BeTrue();
            userType.UpdatedAt.Should().NotBeNull();
            _mockUserTypeRepository.Verify(r => r.UpdateAsync(userType), Times.Once);
        }

        [Fact]
        public async Task ToggleUserTypeStatusAsync_WithNonExistentId_ReturnsFalse()
        {
            // Arrange
            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserType?)null);

            // Act
            var result = await _userTypeService.ToggleUserTypeStatusAsync(999);

            // Assert
            result.Should().BeFalse();
            _mockUserTypeRepository.Verify(r => r.UpdateAsync(It.IsAny<UserType>()), Times.Never);
        }

        [Fact]
        public async Task ToggleUserTypeStatusAsync_MultipleToggles_AlternatesStatus()
        {
            // Arrange
            var userType = CreateUserType(1, "ToggleTest", "Test toggling");
            userType.IsActive = true;

            _mockUserTypeRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(userType);
            _mockUserTypeRepository.Setup(r => r.UpdateAsync(It.IsAny<UserType>()))
                .ReturnsAsync(true);

            // Act & Assert - First toggle
            var result1 = await _userTypeService.ToggleUserTypeStatusAsync(1);
            result1.Should().BeTrue();
            userType.IsActive.Should().BeFalse();

            // Act & Assert - Second toggle
            var result2 = await _userTypeService.ToggleUserTypeStatusAsync(1);
            result2.Should().BeTrue();
            userType.IsActive.Should().BeTrue();

            // Act & Assert - Third toggle
            var result3 = await _userTypeService.ToggleUserTypeStatusAsync(1);
            result3.Should().BeTrue();
            userType.IsActive.Should().BeFalse();

            _mockUserTypeRepository.Verify(r => r.UpdateAsync(userType), Times.Exactly(3));
        }

        #endregion

        #region Helper Methods

        private static UserType CreateUserType(int id, string name, string description)
        {
            return new UserType
            {
                Id = id,
                Name = name,
                Description = description,
                IsActive = true,
                Permissions = "[]",
                CreatedAt = DateTime.UtcNow
            };
        }

        #endregion
    }
}
