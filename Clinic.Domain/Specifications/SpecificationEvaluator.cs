using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Clinic.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Domain.Specifications
{
    public class SpecificationEvaluator<T> where T : BaseEntity
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            var query = inputQuery;

            if (spec.Criteria != null) query = query.Where(spec.Criteria);
            foreach (var include in spec.Includes) query = query.Include(include);
            if (spec.OrderBy != null) query = query.OrderBy(spec.OrderBy);
            if (spec.OrderByDescending != null) query = query.OrderByDescending(spec.OrderByDescending);
            if (spec.IsPagingEnabled && spec.Skip.HasValue && spec.Take.HasValue) query = query.Skip(spec.Skip.Value).Take(spec.Take.Value);



            return query;
        }
    }
}
