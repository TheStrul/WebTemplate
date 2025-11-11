namespace WebTemplate.UnitTests.Services
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System.Linq.Expressions;
    using WebTemplate.Core.DTOs;
    using WebTemplate.Core.DTOs.Auth;
    using WebTemplate.Core.Entities;
    using WebTemplate.Core.Interfaces;
    using WebTemplate.Core.Services;
    using WebTemplate.UnitTests.Fixtures;
    using Xunit;

    public class UserServiceTests : BaseTestFixture
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IUserTypeRepository> _mockUserTypeRepository;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly Mock<IEmailSender> _mockEmailSender;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Setup UserManager mock with required dependencies
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _mockUserTypeRepository = new Mock<IUserTypeRepository>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockEmailSender = new Mock<IEmailSender>();

            _userService = new UserService(
                _mockUserManager.Object,
                _mockUserTypeRepository.Object,
                _mockLogger.Object,
                _mockEmailSender.Object);
        }

        #region GetUserProfileAsync Tests

        [Fact]
        public async Task GetUserProfileAsync_WithValidUser_ReturnsProfile()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "john@test.com", "John", "Doe", userType);
            var roles = new List<string> { "User" };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(roles);

            // Act
            var result = await _userService.GetUserProfileAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Email.Should().Be("john@test.com");
            result.Roles.Should().BeEquivalentTo(roles);
            result.UserType.Id.Should().Be(1);
            result.UserType.Name.Should().Be("Standard");
        }

        [Fact]
        public async Task GetUserProfileAsync_WithNonExistentUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = "non-existent";
            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _userService.GetUserProfileAsync(userId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"User with ID {userId} not found");
        }

        [Fact]
        public async Task GetUserProfileAsync_WithDeletedUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = "deleted-user";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "deleted@test.com", "Deleted", "User", userType);
            user.IsDeleted = true;

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            Func<Task> act = async () => await _userService.GetUserProfileAsync(userId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        #endregion

        #region UpdateUserProfileAsync Tests

        [Fact]
        public async Task UpdateUserProfileAsync_WithValidData_UpdatesAndReturnsTrue()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "john@test.com", "John", "Doe", userType);

            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "Jane",
                LastName = "Smith",
                PhoneNumber = "1234567890",
                Address = "123 Main St",
                City = "TestCity",
                PostalCode = "12345",
                Country = "TestCountry"
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.UpdateUserProfileAsync(userId, updateDto);

            // Assert
            result.Should().BeTrue();
            user.FirstName.Should().Be("Jane");
            user.LastName.Should().Be("Smith");
            user.PhoneNumber.Should().Be("1234567890");
            user.Address.Should().Be("123 Main St");
            user.City.Should().Be("TestCity");
            user.UpdatedAt.Should().NotBeNull();
            _mockUserManager.Verify(um => um.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_WithNonExistentUser_ReturnsFalse()
        {
            // Arrange
            var userId = "non-existent";
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "Test",
                LastName = "User"
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _userService.UpdateUserProfileAsync(userId, updateDto);

            // Assert
            result.Should().BeFalse();
            _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_WithDeletedUser_ReturnsFalse()
        {
            // Arrange
            var userId = "deleted-user";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "test@test.com", "Test", "User", userType);
            user.IsDeleted = true;

            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "New",
                LastName = "Name"
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.UpdateUserProfileAsync(userId, updateDto);

            // Assert
            result.Should().BeFalse();
            _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_WhenUpdateFails_ReturnsFalse()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "test@test.com", "Test", "User", userType);
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "New",
                LastName = "Name"
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

            // Act
            var result = await _userService.UpdateUserProfileAsync(userId, updateDto);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region ChangePasswordAsync Tests

        [Fact]
        public async Task ChangePasswordAsync_WithCorrectCurrentPassword_ReturnsTrue()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "test@test.com", "Test", "User", userType);
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "OldPass123!",
                NewPassword = "NewPass123!"
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.ChangePasswordAsync(userId, changePasswordDto);

            // Assert
            result.Should().BeTrue();
            _mockUserManager.Verify(um => um.ChangePasswordAsync(user, "OldPass123!", "NewPass123!"), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ReturnsFalse()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "test@test.com", "Test", "User", userType);
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "WrongPass",
                NewPassword = "NewPass123!"
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password" }));

            // Act
            var result = await _userService.ChangePasswordAsync(userId, changePasswordDto);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ChangePasswordAsync_WithNonExistentUser_ReturnsFalse()
        {
            // Arrange
            var userId = "non-existent";
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "OldPass123!",
                NewPassword = "NewPass123!"
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _userService.ChangePasswordAsync(userId, changePasswordDto);

            // Assert
            result.Should().BeFalse();
            _mockUserManager.Verify(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_WithValidUser_SoftDeletesAndReturnsTrue()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "test@test.com", "Test", "User", userType);

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            result.Should().BeTrue();
            user.IsDeleted.Should().BeTrue();
            user.IsActive.Should().BeFalse();
            user.DeletedAt.Should().NotBeNull();
            _mockUserManager.Verify(um => um.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_WithNonExistentUser_ReturnsFalse()
        {
            // Arrange
            var userId = "non-existent";

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            result.Should().BeFalse();
            _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUpdateFails_ReturnsFalse()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "test@test.com", "Test", "User", userType);

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Delete failed" }));

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllNonDeletedUsers()
        {
            // Arrange
            var userType = CreateUserType(1, "Standard");
            var user1 = CreateUser("user-1", "user1@test.com", "User", "One", userType);
            var user2 = CreateUser("user-2", "user2@test.com", "User", "Two", userType);
            var user3 = CreateUser("user-3", "user3@test.com", "User", "Three", userType);
            user3.IsDeleted = true; // Should not be returned

            // Mock UserManager.Users as IQueryable
            var users = new List<ApplicationUser> { user1, user2, user3 }.AsQueryable();
            var mockUsers = MockDbSet(users);
            _mockUserManager.Setup(um => um.Users).Returns(mockUsers);

            _mockUserTypeRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<UserType> { userType });

            _mockUserManager.Setup(um => um.GetRolesAsync(user1))
                .ReturnsAsync(new List<string> { "User" });
            _mockUserManager.Setup(um => um.GetRolesAsync(user2))
                .ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(u => u.Email == "user1@test.com");
            result.Should().Contain(u => u.Email == "user2@test.com");
            result.Should().NotContain(u => u.Email == "user3@test.com");
        }

        [Fact]
        public async Task GetAllUsersAsync_WithNoUsers_ReturnsEmptyList()
        {
            // Arrange
            var mockUsers = MockDbSet(Array.Empty<ApplicationUser>().AsQueryable());
            _mockUserManager.Setup(um => um.Users).Returns(mockUsers);
            _mockUserTypeRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<UserType>());

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_WithValidUser_ReturnsAdminUserDto()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "john@test.com", "John", "Doe", userType);
            var roles = new List<string> { "User", "Moderator" };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(roles);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(userId);
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Email.Should().Be("john@test.com");
            result.Roles.Should().BeEquivalentTo(roles);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithNonExistentUser_ReturnsNull()
        {
            // Arrange
            var userId = "non-existent";
            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByIdAsync_WithDeletedUser_ReturnsNull()
        {
            // Arrange
            var userId = "deleted-user";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "deleted@test.com", "Deleted", "User", userType);
            user.IsDeleted = true;

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateUserAsync Tests

        [Fact]
        public async Task CreateUserAsync_WithValidData_CreatesUserAndReturnsId()
        {
            // Arrange
            var userType = CreateUserType(1, "Standard");
            var createDto = new CreateUserDto
            {
                FirstName = "New",
                LastName = "User",
                Email = "new@test.com",
                PhoneNumber = "1234567890",
                UserTypeId = 1,
                Roles = new List<string> { "User" },
                SendWelcomeEmail = false
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(createDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _mockUserTypeRepository.Setup(ur => ur.GetByIdAsync(1))
                .ReturnsAsync(userType);
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((u, p) => u.Id = "new-user-id");
            _mockUserManager.Setup(um => um.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.CreateUserAsync(createDto);

            // Assert
            result.Should().Be("new-user-id");
            _mockUserManager.Verify(um => um.CreateAsync(
                It.Is<ApplicationUser>(u =>
                    u.Email == "new@test.com" &&
                    u.FirstName == "New" &&
                    u.LastName == "User" &&
                    u.EmailConfirmed &&
                    u.IsActive),
                It.IsAny<string>()),
                Times.Once);
            _mockUserManager.Verify(um => um.AddToRolesAsync(It.IsAny<ApplicationUser>(), createDto.Roles), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            var userType = CreateUserType(1, "Standard");
            var existingUser = CreateUser("existing", "existing@test.com", "Existing", "User", userType);
            var createDto = new CreateUserDto
            {
                FirstName = "New",
                LastName = "User",
                Email = "existing@test.com",
                UserTypeId = 1
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(createDto.Email))
                .ReturnsAsync(existingUser);

            // Act
            Func<Task> act = async () => await _userService.CreateUserAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("User with email existing@test.com already exists");
        }

        [Fact]
        public async Task CreateUserAsync_WithInvalidUserType_ThrowsInvalidOperationException()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                FirstName = "New",
                LastName = "User",
                Email = "new@test.com",
                UserTypeId = 999
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(createDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _mockUserTypeRepository.Setup(ur => ur.GetByIdAsync(999))
                .ReturnsAsync((UserType?)null);

            // Act
            Func<Task> act = async () => await _userService.CreateUserAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("UserType with ID 999 not found");
        }

        [Fact]
        public async Task CreateUserAsync_WhenCreationFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var userType = CreateUserType(1, "Standard");
            var createDto = new CreateUserDto
            {
                FirstName = "New",
                LastName = "User",
                Email = "new@test.com",
                UserTypeId = 1
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(createDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _mockUserTypeRepository.Setup(ur => ur.GetByIdAsync(1))
                .ReturnsAsync(userType);
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed" }));

            // Act
            Func<Task> act = async () => await _userService.CreateUserAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to create user: Creation failed");
        }

        [Fact]
        public async Task CreateUserAsync_WithSendWelcomeEmail_SendsEmail()
        {
            // Arrange
            var userType = CreateUserType(1, "Standard");
            var createDto = new CreateUserDto
            {
                FirstName = "New",
                LastName = "User",
                Email = "new@test.com",
                UserTypeId = 1,
                SendWelcomeEmail = true
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(createDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _mockUserTypeRepository.Setup(ur => ur.GetByIdAsync(1))
                .ReturnsAsync(userType);
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((u, p) => u.Id = "new-user-id");
            _mockUserManager.Setup(um => um.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("reset-token");
            _mockEmailSender.Setup(es => es.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.CreateUserAsync(createDto);

            // Assert
            result.Should().Be("new-user-id");
            _mockEmailSender.Verify(es => es.SendAsync(
                "new@test.com",
                "Welcome - Set Your Password",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact]
        public async Task UpdateUserAsync_WithValidData_UpdatesUserAndReturnsTrue()
        {
            // Arrange
            var userId = "user-123";
            var oldUserType = CreateUserType(1, "Standard");
            var newUserType = CreateUserType(2, "Premium");
            var user = CreateUser(userId, "test@test.com", "Test", "User", oldUserType);

            var updateDto = new UpdateUserDto
            {
                FirstName = "Updated",
                LastName = "Name",
                PhoneNumber = "9876543210",
                UserTypeId = 2,
                IsActive = true,
                Roles = new List<string> { "User", "Moderator" }
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserTypeRepository.Setup(ur => ur.GetByIdAsync(2))
                .ReturnsAsync(newUserType);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });
            _mockUserManager.Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(um => um.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateDto);

            // Assert
            result.Should().BeTrue();
            user.FirstName.Should().Be("Updated");
            user.LastName.Should().Be("Name");
            user.UserTypeId.Should().Be(2);
            user.IsActive.Should().BeTrue();
            _mockUserManager.Verify(um => um.AddToRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("Moderator"))), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithNonExistentUser_ReturnsFalse()
        {
            // Arrange
            var userId = "non-existent";
            var updateDto = new UpdateUserDto
            {
                FirstName = "Test",
                LastName = "User",
                UserTypeId = 1,
                IsActive = true
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateDto);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateUserAsync_WithInvalidUserType_ReturnsFalse()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "test@test.com", "Test", "User", userType);
            var updateDto = new UpdateUserDto
            {
                FirstName = "Updated",
                LastName = "Name",
                UserTypeId = 999,
                IsActive = true
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserTypeRepository.Setup(ur => ur.GetByIdAsync(999))
                .ReturnsAsync((UserType?)null);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateDto);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region ActivateUserAsync Tests

        [Fact]
        public async Task ActivateUserAsync_WithInactiveUser_ActivatesAndReturnsTrue()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "test@test.com", "Test", "User", userType);
            user.IsActive = false;

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.ActivateUserAsync(userId);

            // Assert
            result.Should().BeTrue();
            user.IsActive.Should().BeTrue();
            user.UpdatedAt.Should().NotBeNull();
            _mockUserManager.Verify(um => um.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task ActivateUserAsync_WithAlreadyActiveUser_ReturnsTrue()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "test@test.com", "Test", "User", userType);
            user.IsActive = true;

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.ActivateUserAsync(userId);

            // Assert
            result.Should().BeTrue();
            _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task ActivateUserAsync_WithNonExistentUser_ReturnsFalse()
        {
            // Arrange
            var userId = "non-existent";

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _userService.ActivateUserAsync(userId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region DeactivateUserAsync Tests

        [Fact]
        public async Task DeactivateUserAsync_WithActiveUser_DeactivatesAndReturnsTrue()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "test@test.com", "Test", "User", userType);
            user.IsActive = true;

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.DeactivateUserAsync(userId);

            // Assert
            result.Should().BeTrue();
            user.IsActive.Should().BeFalse();
            user.UpdatedAt.Should().NotBeNull();
            _mockUserManager.Verify(um => um.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeactivateUserAsync_WithAlreadyInactiveUser_ReturnsTrue()
        {
            // Arrange
            var userId = "user-123";
            var userType = CreateUserType(1, "Standard");
            var user = CreateUser(userId, "test@test.com", "Test", "User", userType);
            user.IsActive = false;

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.DeactivateUserAsync(userId);

            // Assert
            result.Should().BeTrue();
            _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task DeactivateUserAsync_WithNonExistentUser_ReturnsFalse()
        {
            // Arrange
            var userId = "non-existent";

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _userService.DeactivateUserAsync(userId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Helper Methods

        private static UserType CreateUserType(int id, string name)
        {
            return new UserType
            {
                Id = id,
                Name = name,
                Description = $"{name} user type",
                IsActive = true,
                Permissions = "[\"read\",\"write\"]",
                CreatedAt = DateTime.UtcNow
            };
        }

        private static ApplicationUser CreateUser(string id, string email, string firstName, string lastName, UserType userType)
        {
            return new ApplicationUser
            {
                Id = id,
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                UserTypeId = userType.Id,
                UserType = userType,
                IsActive = true,
                IsDeleted = false,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static IQueryable<T> MockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<IQueryable<T>>();
            var mockAsyncEnumerable = new Mock<IAsyncEnumerable<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            return mockSet.Object;
        }

        // Helper classes for async query provider
        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            internal TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object? Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            {
                return new TestAsyncEnumerable<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                var resultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider)
                    .GetMethod(
                        name: nameof(IQueryProvider.Execute),
                        genericParameterCount: 1,
                        types: new[] { typeof(Expression) })!
                    .MakeGenericMethod(resultType)
                    .Invoke(this, new[] { expression });

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(resultType)
                    .Invoke(null, new[] { executionResult })!;
            }
        }

        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable)
                : base(enumerable)
            { }

            public TestAsyncEnumerable(Expression expression)
                : base(expression)
            { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return ValueTask.FromResult(_inner.MoveNext());
            }

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return ValueTask.CompletedTask;
            }
        }

        #endregion
    }
}
