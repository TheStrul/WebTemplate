using WebTemplate.Core.DTOs;
using WebTemplate.Core.DTOs.Auth;

namespace WebTemplate.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto);
        Task<bool> ChangePasswordAsync(string userId, WebTemplate.Core.DTOs.Auth.ChangePasswordDto changePasswordDto);
        Task<bool> DeleteUserAsync(string userId);
        Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();
        Task<AdminUserDto?> GetUserByIdAsync(string userId);
        Task<string> CreateUserAsync(CreateUserDto createUserDto);
        Task<bool> UpdateUserAsync(string userId, UpdateUserDto updateUserDto);
        Task<bool> ActivateUserAsync(string userId);
        Task<bool> DeactivateUserAsync(string userId);
    }

    public interface IUserTypeService
    {
        Task<IEnumerable<UserTypeDto>> GetAllUserTypesAsync();
        Task<UserTypeDto?> GetUserTypeByIdAsync(int id);
        Task<int> CreateUserTypeAsync(CreateUserTypeDto createUserTypeDto);
        Task<bool> UpdateUserTypeAsync(int id, CreateUserTypeDto updateUserTypeDto);
        Task<bool> DeleteUserTypeAsync(int id);
        Task<bool> ToggleUserTypeStatusAsync(int id);
    }

}