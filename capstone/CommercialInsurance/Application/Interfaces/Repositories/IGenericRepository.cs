// Generic repository contract (Get, GetAll, Add, Update, Delete) shared by all entity-specific repositories.
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // Retrieves a single entity by its string primary key; returns null if not found
        Task<T?> GetByIdAsync(string id);
        // Returns all records of type T from the database (use sparingly on large tables)
        Task<IEnumerable<T>> GetAllAsync();
        // Queries records matching a LINQ predicate expression (e.g., u => u.Role == Admin)
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        // Inserts a new entity and returns it with any database-generated fields populated
        Task<T> AddAsync(T entity);
        // Applies changes to an already-tracked entity and persists them
        Task UpdateAsync(T entity);
        // Permanently deletes the given entity from the database
        Task DeleteAsync(T entity);
    }
}
