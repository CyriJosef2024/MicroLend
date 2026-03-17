using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicroLend.DAL.Repositories
{
    public class EmergencyPoolTransactionRepository
    {
        private readonly MicroLendDbContext _context = new MicroLendDbContext();

        public async Task<List<EmergencyPoolTransaction>> GetAllAsync()
        {
            return await _context.EmergencyPoolTransactions.ToListAsync();
        }

        public async Task AddAsync(EmergencyPoolTransaction tx)
        {
            await _context.EmergencyPoolTransactions.AddAsync(tx);
            await _context.SaveChangesAsync();
        }
    }
}
