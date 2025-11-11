namespace WebTemplate.Core.Interfaces
{
    using WebTemplate.Core.Entities;

    /// <summary>
    /// Repository abstraction for accessing UserType entities without coupling Core to Data
    /// </summary>
    public interface IUserTypeRepository
    {
        Task<UserType?> GetByIdAsync(int id);
        Task<IEnumerable<UserType>> GetAllAsync();
        Task<UserType?> GetByNameAsync(string name);
        Task<int> AddAsync(UserType userType);
        Task<bool> UpdateAsync(UserType userType);
        Task<bool> DeleteAsync(UserType userType);
        Task<int> CountUsersWithTypeAsync(int userTypeId);
    }
}
