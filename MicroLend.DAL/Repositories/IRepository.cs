using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicroLend.DAL.Repositories
{
    /// <summary>
    /// Generic repository interface defining standard CRUD operations.
    /// Implements the Repository Pattern for data access abstraction.
    /// </summary>
    /// <typeparam name="T">The entity type being managed by the repository.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves an entity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity.</param>
        /// <returns>The entity if found, otherwise null.</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all entities of type T.
        /// </summary>
        /// <returns>A list of all entities.</returns>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity in the repository.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity from the repository by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        Task DeleteAsync(int id);
    }
e}