using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Repositories;

namespace Clinic.Domain.Specifications
{
    public class BaseSpecification<T> : ISpecification<T>
    {
        public BaseSpecification(Expression<Func<T, bool>> criteria) => Criteria = criteria;
        
        public Expression<Func<T, bool>> Criteria { get; }

        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDescending { get; private set; }
        public int? Take { get; private set; }
        public int? Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; } = false;



        protected void AddInclude(Expression<Func<T, object>> include) => Includes.Add(include);
        protected void ApplyOrderBy(Expression<Func<T, object>> orderBy) => OrderBy = orderBy;
        protected void ApplyOrderByDesc(Expression<Func<T, object>> orderByDesc) => OrderByDescending = orderByDesc;
        protected void ApplyPaging(int skip, int take) { Skip = skip; Take = take; IsPagingEnabled = true; }
    }
}
