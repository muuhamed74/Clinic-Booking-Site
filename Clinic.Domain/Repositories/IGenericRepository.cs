using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;

namespace Clinic.Domain.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> ListAllAsync();
        Task<T> GetEntityWithSpec(ISpecification<T> spec);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
        Task<IEnumerable<T>> GetAllWithSpecAsync(ISpecification<T> spec);
        Task<int> GetCountWithSpecAsync(ISpecification<T> spec);
        Task<int> MaxAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, int>> selector);

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
