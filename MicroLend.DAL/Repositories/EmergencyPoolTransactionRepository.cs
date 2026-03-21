using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroLend.DAL.Exceptions;

namespace MicroLend.DAL.Repositories
{
    public class EmergencyPoolTransactionRepository
    {
        private readonly MicroLendDbContext _context = new MicroLendDbContext();

        public async Task<List<EmergencyPoolTransaction>> GetAllAsync()
        {
            try
            {
                return await _context.EmergencyPoolTransactions.ToListAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while retrieving all emergency pool transactions", ex);
                throw new DataAccessException("Unable to retrieve transaction data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while retrieving all emergency pool transactions", ex);
                throw new DataAccessException("An unexpected error occurred while accessing transaction data.");
            }
        }

        public async Task AddAsync(EmergencyPoolTransaction tx)
        {
            try
            {
                await _context.EmergencyPoolTransactions.AddAsync(tx);
                await _context.SaveChangesAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while adding new emergency pool transaction", ex);
                throw new DataAccessException("Unable to save transaction data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while adding new emergency pool transaction", ex);
                throw new DataAccessException("An unexpected error occurred while saving transaction data.");
            }
        }
    }
}
