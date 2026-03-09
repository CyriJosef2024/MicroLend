using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroLend.DAL.Repositories
{
    public class EmergencyPoolRepository
    {
        private readonly MicroLendDbContext _context = new MicroLendDbContext();

        public async Task<EmergencyPool> GetPoolAsync()
        {
            return await _context.EmergencyPools.FirstOrDefaultAsync() ?? new EmergencyPool();
        }

        public async Task UpdatePoolAsync(decimal amount)
        {
            var pool = await GetPoolAsync();
            pool.TotalBalance += amount;

            if (pool.Id == 0) _context.EmergencyPools.Add(pool);
            await _context.SaveChangesAsync();
        }
    }
}
