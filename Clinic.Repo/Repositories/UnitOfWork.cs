using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Clinic.Domain.Repositories;
using Clinic.Repo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Clinic.Repo.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ClinicContext _dbContext;
        private Hashtable _repositories;
        public UnitOfWork(ClinicContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Hashtable();
        }

        public async Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
           _dbContext.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _dbContext.DisposeAsync();
        }

        public IGenericRepository<TEntity> Reposit<TEntity>() where TEntity : BaseEntity
        {
            var type = typeof(TEntity).Name;
            if (!_repositories.ContainsKey(type))
            {
                var Repository = new GenericRepository<TEntity>(_dbContext);
                _repositories.Add(type, Repository);
            }
            return (IGenericRepository<TEntity>)_repositories[type];
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            return await _dbContext.Database.BeginTransactionAsync(isolationLevel);
        }
    }
}
