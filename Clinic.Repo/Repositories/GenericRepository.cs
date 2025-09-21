using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Clinic.Domain.Repositories;
using Clinic.Domain.Specifications;
using Clinic.Repo.Data;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Clinic.Repo.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly ClinicContext _context;

        public GenericRepository(ClinicContext context)
        {
            _context = context;
        }

        public async Task<T?> GetByIdAsync(int id) =>
            await _context.Set<T>().FindAsync(id);

        public async Task<IReadOnlyList<T>> GetAllAsync() =>
            await _context.Set<T>().ToListAsync();

        public async Task<IReadOnlyList<T>> ListAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
        {
           return await ApplySpecification(spec).ToListAsync();
        }
        public async Task<T> GetEntityWithSpec(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetAllWithSpecAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task<int> GetCountWithSpecAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).CountAsync();
        }

        public async Task<int> MaxAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, int>> selector)
        {
            var query = _context.Set<T>().Where(predicate);

            if (!await query.AnyAsync())
                return 0;

            return await query.MaxAsync(selector);
        }







        public async Task AddAsync(T entity) =>
            await _context.Set<T>().AddAsync(entity);

        public void Update(T entity) =>
            _context.Set<T>().Update(entity);

        public void Delete(T entity) =>
            _context.Set<T>().Remove(entity);

      






        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
        }

       
    }
}
