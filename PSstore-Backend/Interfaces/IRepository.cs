using System.Linq.Expressions;
using PSstore.DTOs;

namespace PSstore.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Read operations
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();

        // Pagination operations
        /// <summary>
        /// Get paginated results with optional filtering and sorting
        /// </summary>
        Task<PagedResponse<T>> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true);

        // Write operations
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        // Unit of Work
        Task<int> SaveChangesAsync();
    }
}
