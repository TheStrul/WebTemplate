namespace WebTemplate.Core.Services
{
    using Microsoft.Extensions.Logging;
    using System.Text.Json;
    using WebTemplate.Core.DTOs;
    using WebTemplate.Core.Entities;
    using WebTemplate.Core.Interfaces;

    /// <summary>
    /// UserType service implementation for CRUD operations
    /// Handles user type/role management
    /// </summary>
    public class UserTypeService : IUserTypeService
    {
        private readonly IUserTypeRepository _userTypeRepository;
        private readonly ILogger<UserTypeService> _logger;

        public UserTypeService(
            IUserTypeRepository userTypeRepository,
            ILogger<UserTypeService> logger)
        {
            _userTypeRepository = userTypeRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<UserTypeDto>> GetAllUserTypesAsync()
        {
            var userTypes = await _userTypeRepository.GetAllAsync();
            return userTypes.Select(MapToDto).ToList();
        }

        public async Task<UserTypeDto?> GetUserTypeByIdAsync(int id)
        {
            var userType = await _userTypeRepository.GetByIdAsync(id);
            return userType != null ? MapToDto(userType) : null;
        }

        public async Task<int> CreateUserTypeAsync(CreateUserTypeDto createUserTypeDto)
        {
            // Check for duplicate name
            var existingUserType = await _userTypeRepository.GetByNameAsync(createUserTypeDto.Name);
            if (existingUserType != null)
            {
                throw new InvalidOperationException($"UserType with name '{createUserTypeDto.Name}' already exists");
            }

            var userType = new UserType
            {
                Name = createUserTypeDto.Name,
                Description = createUserTypeDto.Description,
                Permissions = JsonSerializer.Serialize(createUserTypeDto.Permissions),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var userTypeId = await _userTypeRepository.AddAsync(userType);
            _logger.LogInformation("UserType {UserTypeId} '{UserTypeName}' created successfully",
                userTypeId, userType.Name);

            return userTypeId;
        }        public async Task<bool> UpdateUserTypeAsync(int id, CreateUserTypeDto updateUserTypeDto)
        {
            var userType = await _userTypeRepository.GetByIdAsync(id);
            if (userType == null)
            {
                _logger.LogWarning("UserType {UserTypeId} not found for update", id);
                return false;
            }

            // Check for duplicate name (excluding current record)
            var duplicateUserType = await _userTypeRepository.GetByNameAsync(updateUserTypeDto.Name);
            if (duplicateUserType != null && duplicateUserType.Id != id)
            {
                throw new InvalidOperationException($"UserType with name '{updateUserTypeDto.Name}' already exists");
            }

            userType.Name = updateUserTypeDto.Name;
            userType.Description = updateUserTypeDto.Description;
            userType.Permissions = JsonSerializer.Serialize(updateUserTypeDto.Permissions);
            userType.UpdatedAt = DateTime.UtcNow;

            await _userTypeRepository.UpdateAsync(userType);
            _logger.LogInformation("UserType {UserTypeId} updated successfully", id);
            return true;
        }

        public async Task<bool> DeleteUserTypeAsync(int id)
        {
            var userType = await _userTypeRepository.GetByIdAsync(id);
            if (userType == null)
            {
                _logger.LogWarning("UserType {UserTypeId} not found for deletion", id);
                return false;
            }

            // Check if any users are assigned to this UserType
            var usersWithType = await _userTypeRepository.CountUsersWithTypeAsync(id);
            if (usersWithType > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot delete UserType '{userType.Name}' because {usersWithType} user(s) are assigned to it");
            }

            await _userTypeRepository.DeleteAsync(userType);
            _logger.LogInformation("UserType {UserTypeId} deleted successfully", id);
            return true;
        }

        public async Task<bool> ToggleUserTypeStatusAsync(int id)
        {
            var userType = await _userTypeRepository.GetByIdAsync(id);
            if (userType == null)
            {
                _logger.LogWarning("UserType {UserTypeId} not found for status toggle", id);
                return false;
            }

            userType.IsActive = !userType.IsActive;
            userType.UpdatedAt = DateTime.UtcNow;

            await _userTypeRepository.UpdateAsync(userType);
            _logger.LogInformation("UserType {UserTypeId} status toggled to {IsActive}",
                id, userType.IsActive);
            return true;
        }

        #region Helper Methods

        private static UserTypeDto MapToDto(UserType userType)
        {
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

        #endregion
    }
}
