namespace WebTemplate.Data.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using WebTemplate.Core.Entities;
    using WebTemplate.Core.Interfaces;
    using WebTemplate.Data.Context;

    public class UserTypeRepository : IUserTypeRepository
    {
        private readonly ApplicationDbContext _db;

        public UserTypeRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UserType?> GetByIdAsync(int id)
        {
            return await _db.UserTypes.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<UserType>> GetAllAsync()
        {
            return await _db.UserTypes.OrderBy(ut => ut.Name).ToListAsync();
        }

        public async Task<UserType?> GetByNameAsync(string name)
        {
            return await _db.UserTypes
                .FirstOrDefaultAsync(ut => ut.Name.ToLower() == name.ToLower());
        }

        public async Task<int> AddAsync(UserType userType)
        {
            _db.UserTypes.Add(userType);
            await _db.SaveChangesAsync();
            return userType.Id;
        }

        public async Task<bool> UpdateAsync(UserType userType)
        {
            _db.UserTypes.Update(userType);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(UserType userType)
        {
            _db.UserTypes.Remove(userType);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountUsersWithTypeAsync(int userTypeId)
        {
            return await _db.Users
                .Where(u => u.UserTypeId == userTypeId && !u.IsDeleted)
                .CountAsync();
        }
    }
}
