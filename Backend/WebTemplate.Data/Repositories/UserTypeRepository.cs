using Microsoft.EntityFrameworkCore;
using WebTemplate.Core.Entities;
using WebTemplate.Core.Interfaces;
using WebTemplate.Data.Context;

namespace WebTemplate.Data.Repositories
{
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
    }
}
