using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Clinic.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TEntity> Reposit<TEntity>() where TEntity : BaseEntity;
        Task<int> CompleteAsync();
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel);
    }
}
