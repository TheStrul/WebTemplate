namespace WebTemplate.Core.Services
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System.Text.Json;
    using WebTemplate.Core.DTOs;
    using WebTemplate.Core.DTOs.Auth;
    using WebTemplate.Core.Entities;
    using WebTemplate.Core.Interfaces;

    /// <summary>
    /// User service implementation for CRUD operations
    /// Handles user management, profile updates, and admin operations
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserTypeRepository _userTypeRepository;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailSender _emailSender;

        public UserService(
            UserManager<ApplicationUser> userManager,
            IUserTypeRepository userTypeRepository,
            ILogger<UserService> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userTypeRepository = userTypeRepository;
            _logger = logger;
            _emailSender = emailSender;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.IsDeleted)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Load UserType separately if not already loaded
            if (user.UserType == null && user.UserTypeId > 0)
            {
                user.UserType = await _userTypeRepository.GetByIdAsync(user.UserTypeId);
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                PhoneNumber2 = user.PhoneNumber2,
                Address = user.Address,
                City = user.City,
                PostalCode = user.PostalCode,
                Country = user.Country,
                ProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                UserType = user.UserType != null ? MapUserTypeToDto(user.UserType) : null,
                Roles = roles.ToList()
            };
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("User {UserId} not found for profile update", userId);
                return false;
            }

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.PhoneNumber = updateDto.PhoneNumber;
            user.PhoneNumber2 = updateDto.PhoneNumber2;
            user.Address = updateDto.Address;
            user.City = updateDto.City;
            user.PostalCode = updateDto.PostalCode;
            user.Country = updateDto.Country;
            user.ProfileImageUrl = updateDto.ProfileImageUrl;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User profile updated successfully for {UserId}", userId);
                return true;
            }

            _logger.LogError("Failed to update user profile for {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("User {UserId} not found for password change", userId);
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                return true;
            }

            _logger.LogWarning("Failed to change password for user {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for deletion", userId);
                return false;
            }

            // Soft delete
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} soft deleted successfully", userId);
                return true;
            }

            _logger.LogError("Failed to delete user {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            // Load all UserTypes once
            var userTypes = (await _userTypeRepository.GetAllAsync()).ToDictionary(ut => ut.Id);

            var userDtos = new List<AdminUserDto>();

            foreach (var user in users)
            {
                // Get UserType from dictionary
                userTypes.TryGetValue(user.UserTypeId, out var userType);

                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new AdminUserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    UserType = userType != null ? MapUserTypeToDto(userType) : null,
                    Roles = roles.ToList()
                });
            }

            return userDtos;
        }

        public async Task<AdminUserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.IsDeleted)
            {
                return null;
            }

            // Load UserType separately if not already loaded
            if (user.UserType == null && user.UserTypeId > 0)
            {
                user.UserType = await _userTypeRepository.GetByIdAsync(user.UserTypeId);
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new AdminUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                UserType = user.UserType != null ? MapUserTypeToDto(user.UserType) : null,
                Roles = roles.ToList()
            };
        }

        public async Task<string> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {createUserDto.Email} already exists");
            }

            // Validate UserType exists
            var userType = await _userTypeRepository.GetByIdAsync(createUserDto.UserTypeId);
            if (userType == null)
            {
                throw new InvalidOperationException($"UserType with ID {createUserDto.UserTypeId} not found");
            }

            // Generate random password
            var randomPassword = GenerateRandomPassword();

            var user = new ApplicationUser
            {
                UserName = createUserDto.Email,
                Email = createUserDto.Email,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                PhoneNumber = createUserDto.PhoneNumber,
                UserTypeId = createUserDto.UserTypeId,
                EmailConfirmed = true, // Admin-created users are pre-confirmed
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, randomPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user {Email}: {Errors}", createUserDto.Email, errors);
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            // Assign roles
            if (createUserDto.Roles.Any())
            {
                await _userManager.AddToRolesAsync(user, createUserDto.Roles);
            }

            _logger.LogInformation("User {UserId} created successfully by admin", user.Id);

            // Send welcome email with password reset link
            if (createUserDto.SendWelcomeEmail)
            {
                try
                {
                    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var plainText = $"Welcome to the system! Please set your password using this link: [Password Reset Link with token: {resetToken}]";
                    await _emailSender.SendAsync(
                        user.Email!,
                        "Welcome - Set Your Password",
                        plainText,
                        plainText);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
                }
            }

            return user.Id;
        }

        public async Task<bool> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("User {UserId} not found for update", userId);
                return false;
            }

            // Validate UserType exists
            var userType = await _userTypeRepository.GetByIdAsync(updateUserDto.UserTypeId);
            if (userType == null)
            {
                _logger.LogWarning("UserType {UserTypeId} not found", updateUserDto.UserTypeId);
                return false;
            }

            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            user.PhoneNumber = updateUserDto.PhoneNumber;
            user.UserTypeId = updateUserDto.UserTypeId;
            user.IsActive = updateUserDto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user {UserId}: {Errors}",
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }

            // Update roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(updateUserDto.Roles).ToList();
            var rolesToAdd = updateUserDto.Roles.Except(currentRoles).ToList();

            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            _logger.LogInformation("User {UserId} updated successfully", userId);
            return true;
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("User {UserId} not found for activation", userId);
                return false;
            }

            if (user.IsActive)
            {
                return true; // Already active
            }

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} activated successfully", userId);
                return true;
            }

            _logger.LogError("Failed to activate user {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("User {UserId} not found for deactivation", userId);
                return false;
            }

            if (!user.IsActive)
            {
                return true; // Already inactive
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} deactivated successfully", userId);
                return true;
            }

            _logger.LogError("Failed to deactivate user {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        #region Helper Methods

        private UserTypeDto? MapUserTypeToDto(UserType? userType)
        {
            if (userType == null)
            {
                return null;
            }

            var permissions = new List<string>();
            if (!string.IsNullOrWhiteSpace(userType.Permissions))
            {
                try
                {
                    permissions = JsonSerializer.Deserialize<List<string>>(userType.Permissions) ?? new List<string>();
                }
                catch (JsonException)
                {
                    // Invalid JSON, return empty list
                }
            }

            return new UserTypeDto
            {
                Id = userType.Id,
                Name = userType.Name,
                Description = userType.Description,
                IsActive = userType.IsActive,
                Permissions = permissions
            };
        }

        private static string GenerateRandomPassword()
        {
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*";

            var random = new Random();
            var password = new char[16];

            // Ensure at least one of each required character type
            password[0] = uppercase[random.Next(uppercase.Length)];
            password[1] = lowercase[random.Next(lowercase.Length)];
            password[2] = digits[random.Next(digits.Length)];
            password[3] = special[random.Next(special.Length)];

            // Fill the rest randomly
            var allChars = uppercase + lowercase + digits + special;
            for (int i = 4; i < password.Length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            // Shuffle the password
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }

        #endregion
    }
}
