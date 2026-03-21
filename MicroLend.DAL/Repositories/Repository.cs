using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroLend.DAL.Exceptions;

namespace MicroLend.DAL.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly MicroLendDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository() : this(new MicroLendDbContext()) { }

    public Repository(MicroLendDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (System.Data.Common.DbException ex)
        {
            Logger.LogError($"Database error while retrieving entity with ID: {id}", ex);
            throw new DataAccessException("Unable to retrieve data. Please check your database connection.");
        }
        catch (System.InvalidOperationException ex)
        {
            Logger.LogError($"Query error while retrieving entity with ID: {id}", ex);
            throw new DataAccessException("Unable to process the query. Please contact support.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Unexpected error while retrieving entity with ID: {id}", ex);
            throw new DataAccessException("An unexpected error occurred while accessing data.");
        }
    }

    public async Task<List<T>> GetAllAsync()
    {
        try
        {
            return await _dbSet.ToListAsync();
        }
        catch (System.Data.Common.DbException ex)
        {
            Logger.LogError("Database error while retrieving all entities", ex);
            throw new DataAccessException("Unable to retrieve data. Please check your database connection.");
        }
        catch (System.InvalidOperationException ex)
        {
            Logger.LogError("Query error while retrieving all entities", ex);
            throw new DataAccessException("Unable to process the query. Please contact support.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError("Unexpected error while retrieving all entities", ex);
            throw new DataAccessException("An unexpected error occurred while accessing data.");
        }
    }

    public async Task AddAsync(T entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        catch (System.Data.Common.DbException ex)
        {
            Logger.LogError("Database error while adding new entity", ex);
            throw new DataAccessException("Unable to save data. Please check your database connection.");
        }
        catch (System.InvalidOperationException ex)
        {
            Logger.LogError("Query error while adding new entity", ex);
            throw new DataAccessException("Unable to process the operation. Please contact support.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError("Unexpected error while adding new entity", ex);
            throw new DataAccessException("An unexpected error occurred while saving data.");
        }
    }

    public async Task UpdateAsync(T entity)
    {
        try
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }
        catch (System.Data.Common.DbException ex)
        {
            Logger.LogError("Database error while updating entity", ex);
            throw new DataAccessException("Unable to update data. Please check your database connection.");
        }
        catch (System.InvalidOperationException ex)
        {
            Logger.LogError("Query error while updating entity", ex);
            throw new DataAccessException("Unable to process the operation. Please contact support.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError("Unexpected error while updating entity", ex);
            throw new DataAccessException("An unexpected error occurred while updating data.");
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        try
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        catch (System.Data.Common.DbException ex)
        {
            Logger.LogError($"Database error while deleting entity with ID: {id}", ex);
            throw new DataAccessException("Unable to delete data. Please check your database connection.");
        }
        catch (System.InvalidOperationException ex)
        {
            Logger.LogError($"Query error while deleting entity with ID: {id}", ex);
            throw new DataAccessException("Unable to process the operation. Please contact support.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Unexpected error while deleting entity with ID: {id}", ex);
            throw new DataAccessException("An unexpected error occurred while deleting data.");
        }
    }
}