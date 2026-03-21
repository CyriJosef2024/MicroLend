using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using MicroLend.DAL.Exceptions;

namespace MicroLend.DAL.Repositories;

public class UserRepository : Repository<User>
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        try
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }
        catch (System.Data.Common.DbException ex)
        {
            Logger.LogError($"Database error while retrieving user by username: {username}", ex);
            throw new DataAccessException("Unable to retrieve user data. Please check your database connection.");
        }
        catch (System.InvalidOperationException ex)
        {
            Logger.LogError($"Query error while retrieving user by username: {username}", ex);
            throw new DataAccessException("Unable to process user query. Please contact support.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Unexpected error while retrieving user by username: {username}", ex);
            throw new DataAccessException("An unexpected error occurred while accessing user data.");
        }
    }
}
