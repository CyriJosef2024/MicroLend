using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using MicroLend.DAL.Exceptions;

namespace MicroLend.DAL.Repositories
{
    public class BorrowerRepository
    {
        private readonly MicroLendDbContext _context = new MicroLendDbContext();

        public async Task<List<Borrower>> GetAllAsync()
        {
            try
            {
                return await _context.Borrowers.ToListAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while retrieving all borrowers", ex);
                throw new DataAccessException("Unable to retrieve borrower data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while retrieving all borrowers", ex);
                throw new DataAccessException("An unexpected error occurred while accessing borrower data.");
            }
        }

        public async Task<Borrower?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Borrowers.FindAsync(id);
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while retrieving borrower ID: {id}", ex);
                throw new DataAccessException("Unable to retrieve borrower data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while retrieving borrower ID: {id}", ex);
                throw new DataAccessException("An unexpected error occurred while accessing borrower data.");
            }
        }

        public async Task AddAsync(Borrower borrower)
        {
            try
            {
                await _context.Borrowers.AddAsync(borrower);
                await _context.SaveChangesAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while adding new borrower", ex);
                throw new DataAccessException("Unable to save borrower data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while adding new borrower", ex);
                throw new DataAccessException("An unexpected error occurred while saving borrower data.");
            }
        }

        public async Task UpdateAsync(Borrower borrower)
        {
            try
            {
                _context.Borrowers.Update(borrower);
                await _context.SaveChangesAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while updating borrower ID: {borrower.Id}", ex);
                throw new DataAccessException("Unable to update borrower data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while updating borrower ID: {borrower.Id}", ex);
                throw new DataAccessException("An unexpected error occurred while updating borrower data.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var b = await _context.Borrowers.FindAsync(id);
                if (b == null) return;
                _context.Borrowers.Remove(b);
                await _context.SaveChangesAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while deleting borrower ID: {id}", ex);
                throw new DataAccessException("Unable to delete borrower data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while deleting borrower ID: {id}", ex);
                throw new DataAccessException("An unexpected error occurred while deleting borrower data.");
            }
        }
    }
}
