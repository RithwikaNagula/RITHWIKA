// Base repository implementation using EF Core: provides GetById, GetAll, Add, Update, and Delete backed by the DbContext.
using System.Linq.Expressions;
using Application.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    // Base class for all entity repositories; provides common CRUD operations backed by EF Core
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly InsuranceDbContext _context;
        // Typed DbSet<T> shortcut so subclasses don't repeatedly call context.Set<T>()
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(InsuranceDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // Finds a single entity by primary key; returns null if no match (uses EF FindAsync for cache hits)
        public virtual async Task<T?> GetByIdAsync(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        // Returns all rows as an untracked list; suitable for read-only listing pages
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        // Queries with a LINQ predicate (e.g., u => u.Role == Admin); results are untracked
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
        }

        // Inserts a new entity and immediately persists via SaveChangesAsync; returns the entity with DB-generated fields
        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // Marks all changed properties on the entity as modified and persists updates
        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        // Removes the entity from the context and issues a DELETE statement
        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
