using WebTemplate.Core.Entities;

namespace WebTemplate.Core.Interfaces
{
    /// <summary>
    /// Repository abstraction for accessing UserType entities without coupling Core to Data
    /// </summary>
    public interface IUserTypeRepository
    {
        Task<UserType?> GetByIdAsync(int id);
    }
}
