using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using MicroLend.DAL.Exceptions;

namespace MicroLend.DAL.Repositories
{
    public class EmergencyPoolRepository
    {
        private readonly MicroLendDbContext _context = new MicroLendDbContext();

        public async Task<EmergencyPool> GetPoolAsync()
        {
            try
            {
                return await _context.EmergencyPools.FirstOrDefaultAsync() ?? new EmergencyPool();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while retrieving emergency pool", ex);
                throw new DataAccessException("Unable to retrieve emergency pool data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while retrieving emergency pool", ex);
                throw new DataAccessException("An unexpected error occurred while accessing emergency pool data.");
            }
        }

        public async Task UpdatePoolAsync(decimal amount)
        {
            try
            {
                var pool = await GetPoolAsync();
                pool.TotalBalance += amount;

                if (pool.Id == 0) _context.EmergencyPools.Add(pool);
                await _context.SaveChangesAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while updating emergency pool", ex);
                throw new DataAccessException("Unable to update emergency pool. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while updating emergency pool", ex);
                throw new DataAccessException("An unexpected error occurred while updating emergency pool data.");
            }
        }
    }
}
