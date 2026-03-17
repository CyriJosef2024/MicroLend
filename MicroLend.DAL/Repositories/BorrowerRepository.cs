using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroLend.DAL.Repositories
{
    public class BorrowerRepository
    {
        private readonly MicroLendDbContext _context = new MicroLendDbContext();

        public async Task<List<Borrower>> GetAllAsync() => await _context.Borrowers.ToListAsync();

        public async Task<Borrower?> GetByIdAsync(int id) => await _context.Borrowers.FindAsync(id);

        public async Task AddAsync(Borrower borrower)
        {
            await _context.Borrowers.AddAsync(borrower);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Borrower borrower)
        {
            _context.Borrowers.Update(borrower);
            await _context.SaveChangesAsync();
        }
    }
}
