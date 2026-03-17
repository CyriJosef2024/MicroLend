using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroLend.DAL.Repositories
{
    public class EmergencyPoolTransactionRepository : Repository<EmergencyPoolTransaction>
    {
        public EmergencyPoolTransactionRepository() : base() { }

        public async Task<List<EmergencyPoolTransaction>> GetByUserIdAsync(int userId)
        {
            return await _dbSet.Where(t => t.UserId == userId).ToListAsync();
        }
    }
}
